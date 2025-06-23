using Azure.Identity;
using Insights.Agents;
using Insights.Endpoints;
using Insights.Models;

var builder = WebApplication.CreateBuilder(args);

// add aspire service defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddOpenApi();

// Disable Globalization Invariant Mode
Environment.SetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", "false");

// Add DbContext service
builder.AddSqlServerDbContext<Context>("insightsdb");

// in dev scenarios rename this to "openaidev", and check the documentation to reuse existing AOAI resources
var azureOpenAiClientName = "openai";
var chatDeploymentName = builder.Configuration["AI_ChatDeploymentName"] ?? "gpt-4.1-mini";
builder.AddAzureOpenAIClient(azureOpenAiClientName, configureSettings: settings =>
{
    settings.Credential = new AzureCliCredential();
}).AddChatClient(chatDeploymentName);

builder.Services.AddAgents();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapInsightsEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// log Azure OpenAI resources
app.Logger.LogInformation("Azure OpenAI resources\n >> OpenAI Client Name: {azureOpenAiClientName}", azureOpenAiClientName);

// manage db
var scope = app.Services.CreateScope();
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

app.Run();
