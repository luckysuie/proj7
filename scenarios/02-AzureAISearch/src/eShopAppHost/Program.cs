using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var appInsights = builder.AddAzureApplicationInsights("appInsights");

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

var chatDeploymentName = "gpt-4o-mini";
var embeddingsDeploymentName = "text-embedding-ada-002";
var aoai = builder.AddAzureOpenAI("openai")
    .AddDeployment(new AzureOpenAIDeployment(chatDeploymentName,
    "gpt-4o-mini",
    "2024-07-18",
    "GlobalStandard",
    10))
    .AddDeployment(new AzureOpenAIDeployment(embeddingsDeploymentName,
    "text-embedding-ada-002",
    "2"));

var azureaisearch = builder.AddAzureSearch("azureaisearch");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WithReference(azureaisearch)
    .WithReference(appInsights)
    .WithReference(aoai)
    .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
    .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WithReference(azureaisearch)
    .WithReference(appInsights)
    .WithExternalHttpEndpoints();

// comment the previous lines and uncomment this for local dev using existing services for Azure OpenAI and Azure AI Search
//var sqldb = builder.AddSqlServer("sql")
//    .WithDataVolume()
//    .AddDatabase("sqldb");
//var products = builder.AddProject<Projects.Products>("products")
//    .WithReference(sqldb);
//var store = builder.AddProject<Projects.Store>("store")
//    .WithReference(products)
//    .WithExternalHttpEndpoints();

builder.Build().Run();
