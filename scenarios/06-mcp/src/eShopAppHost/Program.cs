var builder = DistributedApplication.CreateBuilder(args);

var sqldb = builder.AddSqlServer("sql")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("sqldb");

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(sqldb)
    .WaitFor(sqldb);

var onlineresearcher = builder.AddProject<Projects.OnlineResearcher>("onlineresearcher")
    .WithExternalHttpEndpoints();

var parkinformationagent = builder.AddProject<Projects.ParkInformationAgent>("parkinformationagent")
    .WithExternalHttpEndpoints();

var weatheragent = builder.AddProject<Projects.WeatherAgent>("weatheragent")
    .WithExternalHttpEndpoints();

var eshopmcpserver = builder.AddProject<Projects.eShopMcpSseServer>("eshopmcpserver")
    .WithReference(onlineresearcher)
    .WaitFor(onlineresearcher)
    .WithReference(products)
    .WaitFor(products)
    .WithReference(weatheragent)
    .WaitFor(weatheragent)
    .WithReference(parkinformationagent)
    .WaitFor(parkinformationagent)
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(eshopmcpserver)
    .WaitFor(eshopmcpserver)
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // production code uses Azure services, so we need to add them here
    var appInsights = builder.AddAzureApplicationInsights("appInsights");
    var chatDeploymentName = "gpt-41-mini";
    var embeddingsDeploymentName = "text-embedding-ada-002";
    var aoai = builder.AddAzureOpenAI("openai");
    var gpt41mini = aoai.AddDeployment(name: chatDeploymentName,
            modelName: "gpt-4.1-mini",
            modelVersion: "2025-04-14");
    gpt41mini.Resource.SkuCapacity = 10;
    gpt41mini.Resource.SkuName = "GlobalStandard";

    var embeddingsDeployment = aoai.AddDeployment(name: embeddingsDeploymentName,
        modelName: "text-embedding-ada-002",
        modelVersion: "2");

    products.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

    eshopmcpserver.WithReference(appInsights)
        .WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

    store.WithReference(appInsights)
        .WithReference(aoai)
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
