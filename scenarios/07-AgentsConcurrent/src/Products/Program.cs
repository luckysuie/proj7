using Azure.Identity;
using Products.Endpoints;
using Products.Memory;
using Products.Models;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add DbContext service
builder.AddSqlServerDbContext<Context>("productsdb");

var azureOpenAiClientName = "openai";
var embeddingsDeploymentName = builder.Configuration["AI_embeddingsDeploymentName"] ?? "text-embedding-ada-002";
builder.AddAzureOpenAIClient(azureOpenAiClientName, configureSettings: settings =>
{
    settings.Credential = new AzureCliCredential();
}).AddEmbeddingGenerator(embeddingsDeploymentName);

var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "gpt-4.1-mini";
builder.AddAzureOpenAIClient(azureOpenAiClientName, configureSettings: settings =>
{
    settings.Credential = new AzureCliCredential();
}).AddChatClient(chatDeploymentName);

// add memory context
builder.Services.AddSingleton<MemoryContext>();

// Add services to the container.
var app = builder.Build();

// aspire map default endpoints
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.MapProductEndpoints();

app.UseStaticFiles();

// log Azure OpenAI resources
app.Logger.LogInformation("Azure OpenAI resources\n >> OpenAI Client Name: {azureOpenAiClientName}", azureOpenAiClientName);

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
    DbInitializer.Initialize(context);

    app.Logger.LogInformation("Start fill products in vector db");
    var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    await memoryContext.InitMemoryContextAsync(context);
    app.Logger.LogInformation("Done fill products in vector db");
}

app.Run();