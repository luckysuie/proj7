using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// add support for Azure AppService 
var appsvc = builder.AddAzureAppServiceEnvironment("appsvc");

var products = builder.AddProject<Projects.Products>("products")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var store = builder.AddProject<Projects.Store>("store")
    .WithReference(products)
    .WaitFor(products)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

if (builder.ExecutionContext.IsPublishMode)
{
    // production code uses Azure services, so we need to add them here
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


    products.WithReference(aoai)
        .WithEnvironment("AI_ChatDeploymentName", chatDeploymentName)
        .WithEnvironment("AI_embeddingsDeploymentName", embeddingsDeploymentName);

    store.WithExternalHttpEndpoints();
}

builder.Build().Run();
