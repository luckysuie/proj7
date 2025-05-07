using eShopMcpSseServer.Tools;
using McpSample.AspNetCoreSseServer;
using OpenAI;
using OpenAI.Chat;
using Services;

var builder = WebApplication.CreateBuilder(args);

// Add default services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// add product service
builder.Services.AddSingleton<ProductService>();
builder.Services.AddHttpClient<ProductService>(
    static client => client.BaseAddress = new("https+http://products"));

builder.Services.AddSingleton<OnlineResearcherService>();
builder.Services.AddHttpClient<OnlineResearcherService>(
    static client => client.BaseAddress = new("https+http://onlineresearcher"));

builder.Services.AddSingleton<WeatherService>();
builder.Services.AddHttpClient<WeatherService>(
    static client => client.BaseAddress = new("https+http://weatheragent"));

builder.Services.AddSingleton<ParkInformationService>();
builder.Services.AddHttpClient<ParkInformationService>(
    static client => client.BaseAddress = new("https+http://parkinformationagent"));

var azureOpenAiClientName = "openai";
var chatDeploymentName = "gpt-4.1-mini";
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
        logger.LogError(exc, "Error creating embeddings client");
    }
    return chatClient;
});

// add MCP server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<OnlineResearch>()
    .WithTools<ParkInformation>()
    .WithTools<Products>()
    .WithTools<WeatherTool>();

var app = builder.Build();

// Initialize default endpoints
app.MapDefaultEndpoints();
app.UseHttpsRedirection();

// map endpoints
app.MapGet("/", () => $"eShopLite-MCP Server! {DateTime.Now}");
app.MapMcp();

app.Run();
