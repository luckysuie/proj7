using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Products.Endpoints;
using Products.Memory;
using Products.Models;
using Microsoft.Extensions.AI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("productsDb");

// Check if we should use GitHub models (local development) or Azure OpenAI (production)
var useGitHubModels = builder.Configuration.GetValue<bool>("AI_UseGitHubModels", false);

if (useGitHubModels)
{
    // Local development with GitHub models
    var githubToken = builder.Configuration["GitHubToken"];
    
    // Register the legacy ChatClient for backward compatibility
    builder.Services.AddSingleton<ChatClient>(serviceProvider =>
    {
        var logger = serviceProvider.GetService<ILogger<Program>>()!;
        logger.LogInformation("Configuring GitHub Models Chat client for local development");
        
        if (string.IsNullOrEmpty(githubToken))
        {
            logger.LogError("GitHub token is required for local development with GitHub models");
            return null!;
        }
        
        var client = new OpenAIClient(new ApiKeyCredential(githubToken), new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.inference.ai.azure.com")
        });

        // reference: https://github.com/marketplace/models/azure-openai/gpt-4-1-mini
        return client.GetChatClient("gpt-4.1-mini");
    });
    
    // Register the legacy EmbeddingClient for backward compatibility
    builder.Services.AddSingleton<EmbeddingClient>(serviceProvider =>
    {
        var logger = serviceProvider.GetService<ILogger<Program>>()!;
        logger.LogInformation("Configuring GitHub Models Embedding client for local development");
        
        if (string.IsNullOrEmpty(githubToken))
        {
            logger.LogError("GitHub token is required for local development with GitHub models");
            return null!;
        }
        
        var client = new OpenAIClient(new ApiKeyCredential(githubToken), new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.inference.ai.azure.com")
        });

        // reference: https://github.com/marketplace/models/azure-openai/text-embedding-3-small
        return client.GetEmbeddingClient("text-embedding-3-small");
    });
}
else
{
    // Production with Azure OpenAI
    // in dev scenarios rename this to "openaidev", and check the documentation to reuse existing AOAI resources
    var azureOpenAiClientName = "openai";
    var chatDeploymentName = "gpt-4.1-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";
    builder.AddAzureOpenAIClient(azureOpenAiClientName);

    // get azure openai client and create Chat client from aspire hosting configuration
    builder.Services.AddSingleton<ChatClient>(serviceProvider =>
    {
        var logger = serviceProvider.GetService<ILogger<Program>>()!;
        logger.LogInformation($"Chat client configuration, modelId: {chatDeploymentName}");
        ChatClient chatClient = null;
        try
        {
            OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
            chatClient = client.GetChatClient(chatDeploymentName);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Error creating chat client");
        }
        return chatClient;
    });

    // get azure openai client and create embedding client from aspire hosting configuration
    builder.Services.AddSingleton<EmbeddingClient>(serviceProvider =>
    {
        var logger = serviceProvider.GetService<ILogger<Program>>()!;
        logger.LogInformation($"Embeddings client configuration, modelId: {embeddingsDeploymentName}");
        EmbeddingClient embeddingsClient = null;
        try
        {
            OpenAIClient client = serviceProvider.GetRequiredService<OpenAIClient>();
            embeddingsClient = client.GetEmbeddingClient(embeddingsDeploymentName);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Error creating embeddings client");
        }
        return embeddingsClient;
    });
}

builder.Services.AddSingleton<IConfiguration>(sp =>
{
    return builder.Configuration;
});

// add memory context
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetService<ILogger<Program>>();
    logger.LogInformation("Creating memory context");
    
    // Try to get legacy clients first (for Azure OpenAI)
    var chatClient = sp.GetService<ChatClient>();
    var embeddingClient = sp.GetService<EmbeddingClient>();
    
    return new MemoryContext(logger, chatClient, embeddingClient);
});

// Add services to the container.
var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapProductEndpoints();

app.UseStaticFiles();

// log AI configuration
var isUsingGitHubModels = builder.Configuration.GetValue<bool>("AI_UseGitHubModels", false);
if (isUsingGitHubModels)
{
    app.Logger.LogInformation("Using GitHub Models for local development");
}
else
{
    app.Logger.LogInformation("Using Azure OpenAI for production");
}
AppContext.SetSwitch("OpenAI.Experimental.EnableOpenTelemetry", true);

// manage db
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    try
    {
        app.Logger.LogInformation("Ensure database created");
        context.Database.EnsureCreated();
    }
    catch (Exception exc)
    {
        app.Logger.LogError(exc, "Error creating database");
    }
    DbInitializer.Initialize(context, isUsingGitHubModels);

    app.Logger.LogInformation("Start fill products in vector db");
    var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    await memoryContext.InitMemoryContextAsync(context);
    app.Logger.LogInformation("Done fill products in vector db");
}

app.Run();