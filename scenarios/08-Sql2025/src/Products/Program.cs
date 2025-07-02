using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;
using Products.Endpoints;
using Products.Models;

var builder = WebApplication.CreateBuilder(args);

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Context:
// Aspire standard add DbContext service, does not support configuration for vector search
// builder.AddSqlServerDbContext<Context>("productsDb");

// Workaround:
// Get the connection string from configuration, init DbContext and enable vector search
var productsDbConnectionString = builder.Configuration.GetConnectionString("productsDb");
builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));

var azureOpenAiClientName = "openai";
var embeddingsDeploymentName = builder.Configuration["AI_embeddingsDeploymentName"] ?? "text-embedding-3-small";
builder.AddAzureOpenAIClient(azureOpenAiClientName, configureSettings: settings =>
{
    settings.Credential = new AzureCliCredential();
}).AddEmbeddingGenerator(embeddingsDeploymentName);

var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "gpt-4.1-mini";
builder.AddAzureOpenAIClient(azureOpenAiClientName, configureSettings: settings =>
{
    settings.Credential = new AzureCliCredential();
}).AddChatClient(chatDeploymentName);

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
    await DbInitializer.Initialize(context, app.Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>());

    //app.Logger.LogInformation("Start fill products in vector db");
    //var memoryContext = app.Services.GetRequiredService<MemoryContext>();
    //await memoryContext.InitMemoryContextAsync(context);
    //app.Logger.LogInformation("Done fill products in vector db");
}

app.Run();