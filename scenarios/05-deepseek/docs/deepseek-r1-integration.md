# DeepSeek-R1 Integration

## Overview

The DeepSeek-R1 integration showcases advanced AI reasoning capabilities by incorporating DeepSeek's state-of-the-art reasoning model alongside traditional Azure OpenAI services, enabling sophisticated analysis and enhanced product discovery.

## Model Configuration

### DeepSeek-R1 Client Setup

The Products service configures a dedicated DeepSeek-R1 client using keyed services:

```csharp
const string secretSectionNameDeepSeekR1 = "deepseekr1";
const string deploymentNameDeepSeekR1 = "DeepSeek-R1";
const string chatClientNameDeepSeekR1 = "chatClientDeepSeekR1";

// Register ChatClient for DeepSeek-R1
builder.Services.AddActivatedKeyedSingleton<ChatClient>(chatClientNameDeepSeekR1, (sp, _) =>
{
    var logger = sp.GetService<ILogger<Program>>()!;
    logger.LogInformation($"Register ChatClient for DeepSeekR1");
    logger.LogInformation($"Chat client configuration, modelId: {deploymentNameDeepSeekR1}");
    
    try
    {
        var (endpoint, apiKey) = GetEndpointAndKey(builder, secretSectionNameDeepSeekR1, logger);
        
        if (string.IsNullOrEmpty(apiKey))
        {
            // Use Azure Default Credential for managed identity
            var endpointModel = new Uri(endpoint);
            var credential = new DefaultAzureCredential();
            var client = new AzureOpenAIClient(endpoint: endpointModel, credential: credential);
            return client.GetChatClient(deploymentNameDeepSeekR1);
        }
        else
        {
            // Use API key authentication
            var endpointModel = new Uri(endpoint);
            var client = new AzureOpenAIClient(endpoint: endpointModel, new ApiKeyCredential(apiKey));
            return client.GetChatClient(deploymentNameDeepSeekR1);
        }
    }
    catch (Exception exc)
    {
        logger.LogError(exc, "Error creating DeepSeek-R1 chat client");
        throw;
    }
});
```

## Authentication Strategies

### Flexible Authentication Configuration
The integration supports multiple authentication methods:

#### API Key Authentication
```csharp
if (!string.IsNullOrEmpty(apiKey))
{
    var client = new AzureOpenAIClient(
        endpoint: new Uri(endpoint), 
        new ApiKeyCredential(apiKey));
    return client.GetChatClient(deploymentNameDeepSeekR1);
}
```

#### Managed Identity Authentication
```csharp
if (string.IsNullOrEmpty(apiKey))
{
    var credential = new DefaultAzureCredential();
    var client = new AzureOpenAIClient(
        endpoint: new Uri(endpoint), 
        credential: credential);
    return client.GetChatClient(deploymentNameDeepSeekR1);
}
```

## Configuration Management

### Helper Method for Endpoint Resolution
```csharp
static (string endpoint, string apiKey) GetEndpointAndKey(WebApplicationBuilder builder, string name, ILogger<Program> logger)
{
    var (endpoint, apiKey) = GetEndpointAndKey(builder, name, logger);
    return string.IsNullOrEmpty(apiKey)
        ? new AzureOpenAIConfig
        {
            Endpoint = endpoint,
            Auth = AzureOpenAIConfig.AuthTypes.AzureIdentity
        }
        : new AzureOpenAIConfig
        {
            Endpoint = endpoint,
            APIKey = apiKey,
            Auth = AzureOpenAIConfig.AuthTypes.APIKey
        };
}
```

### Connection String Parsing
The system parses connection strings to extract endpoint and authentication information:

```csharp
public static (string endpoint, string apiKey) GetEndpointAndKey(WebApplicationBuilder builder, string name, ILogger<Program> logger)
{
    string? connectionString = builder.Configuration.GetConnectionString(name);
    if (string.IsNullOrEmpty(connectionString))
    {
        logger.LogError($"Connection string for '{name}' not found.");
        return (string.Empty, string.Empty);
    }

    // Parse endpoint and API key from connection string
    var endpointMatch = Regex.Match(connectionString, @"Endpoint=(https://[^\s;]+)");
    var keyMatch = Regex.Match(connectionString, @"Key=([^;]+)");

    string endpoint = endpointMatch.Success ? endpointMatch.Groups[1].Value : string.Empty;
    string apiKey = keyMatch.Success ? keyMatch.Groups[1].Value : string.Empty;

    logger.LogInformation($"Parsed endpoint: {endpoint}");
    logger.LogInformation($"API key present: {!string.IsNullOrEmpty(apiKey)}");

    return (endpoint, apiKey);
}
```

## Model Capabilities

### DeepSeek-R1 Reasoning Features
- **Advanced Reasoning**: Multi-step logical analysis for complex queries
- **Context Understanding**: Deep comprehension of user intent and product relationships
- **Analytical Processing**: Sophisticated product matching and recommendation logic
- **Chain-of-Thought**: Explicit reasoning steps for transparent decision making

### Integration with Memory Context
The DeepSeek-R1 client integrates with the memory context for enhanced search:

```csharp
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context with DeepSeek-R1 support");
    
    // Get both chat clients
    var openAIClient = sp.GetRequiredKeyedService<ChatClient>(chatClientNameOpenAI);
    var deepSeekClient = sp.GetRequiredKeyedService<ChatClient>(chatClientNameDeepSeekR1);
    var embeddingClient = sp.GetService<EmbeddingClient>();
    
    return new MemoryContext(logger, openAIClient, deepSeekClient, embeddingClient);
});
```

## Usage Patterns

### Reasoning-Enhanced Search
The DeepSeek-R1 model can be used for complex product analysis:

```csharp
public async Task<SearchResult> AdvancedSearch(string query, Context db)
{
    // Use DeepSeek-R1 for complex reasoning about the query
    var reasoningPrompt = $"Analyze this product search query and provide detailed reasoning about what the customer is looking for: {query}";
    
    var chatCompletion = await _deepSeekClient.CompleteChatAsync(
        new ChatMessage[] { new SystemChatMessage(reasoningPrompt) });
    
    // Use the reasoning results to enhance search
    var enhancedQuery = chatCompletion.Value.Content[0].Text;
    
    // Perform vector search with enhanced understanding
    return await PerformEnhancedVectorSearch(enhancedQuery, db);
}
```

### Dual AI Strategy Implementation
The system can intelligently route queries to the most appropriate AI model:

```csharp
public async Task<string> ProcessQuery(string query, QueryComplexity complexity)
{
    return complexity switch
    {
        QueryComplexity.Simple => await ProcessWithOpenAI(query),
        QueryComplexity.Complex => await ProcessWithDeepSeek(query),
        QueryComplexity.Reasoning => await ProcessWithDeepSeekReasoning(query),
        _ => await ProcessWithOpenAI(query)
    };
}
```

## Production Deployment

### Azure Container Apps Configuration
In production, both AI services are configured through Aspire:

```csharp
if (builder.ExecutionContext.IsPublishMode)
{
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var aoai = builder.AddAzureOpenAI("openai");
    
    // Configure standard Azure OpenAI deployments
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
        modelName: "gpt-4.1-mini",
        modelVersion: "2025-04-14");
    
    // DeepSeek-R1 would be configured through separate Azure OpenAI instance
    // or through external service configuration
    
    products.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);
}
```

## Performance Considerations

### Model Selection Strategy
- **Standard Queries**: Use Azure OpenAI for speed and efficiency
- **Complex Analysis**: Route to DeepSeek-R1 for advanced reasoning
- **Hybrid Approach**: Combine both models for optimal results

### Caching and Optimization
```csharp
// Cache reasoning results for frequently asked complex queries
public async Task<string> GetCachedReasoning(string query)
{
    var cacheKey = $"deepseek_reasoning_{query.GetHashCode()}";
    
    if (_cache.TryGetValue(cacheKey, out string cachedResult))
    {
        return cachedResult;
    }
    
    var reasoning = await _deepSeekClient.CompleteChatAsync(query);
    _cache.Set(cacheKey, reasoning, TimeSpan.FromHours(1));
    
    return reasoning;
}
```

## Error Handling and Resilience

### Fallback Strategy
```csharp
public async Task<ChatCompletion> GetChatCompletionWithFallback(string query)
{
    try
    {
        // Try DeepSeek-R1 first for complex reasoning
        return await _deepSeekClient.CompleteChatAsync(query);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "DeepSeek-R1 unavailable, falling back to Azure OpenAI");
        
        // Fallback to Azure OpenAI
        return await _openAIClient.CompleteChatAsync(query);
    }
}
```

### Health Monitoring
- Monitor response times and success rates for both AI providers
- Implement circuit breaker patterns for resilience
- Track model performance and quality metrics

## Dependencies

### Required Packages
- `Azure.AI.OpenAI` - Azure OpenAI client libraries
- `Azure.Identity` - Azure authentication libraries
- `Microsoft.KernelMemory` - Vector operations support

### External Services
- Azure OpenAI Service (for embeddings and standard chat)
- DeepSeek-R1 service endpoint
- Azure Application Insights for monitoring
- Azure Managed Identity for secure authentication