using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SemanticSearchFunction.Functions;
using SemanticSearchFunction.Repositories;
using SemanticSearchFunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // add db with support for vector search
        var productsDbConnectionString = context.Configuration.GetConnectionString("productsDb");
        services.AddDbContext<Context>(options =>
            options.UseSqlServer(productsDbConnectionString, o => o.UseVectorSearch()));

        // add embedding generator
        var embeddingsDeploymentName = context.Configuration["AI_embeddingsDeploymentName"] ?? "text-embedding-3-small";
        services.AddSingleton(_ =>
            AzureOpenAiEmbeddingProvider.CreateEmbeddingClient(context.Configuration, embeddingsDeploymentName));

        // add an instance of SqlSemanticSearchRepository so it can be injected into the function
        services.AddScoped(sp =>
        {
            var dbContext = sp.GetRequiredService<Context>();
            var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var db = sp.GetRequiredService<Context>();
            var logger = sp.GetRequiredService<ILogger<SearchFunction>>();            
            return new SqlSemanticSearchRepository(embeddingGenerator, db, logger);
        });

    })
    .Build();

host.Run();