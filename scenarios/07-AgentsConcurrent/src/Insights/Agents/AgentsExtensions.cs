using Microsoft.SemanticKernel;

namespace Insights.Agents;

public static class AgentsExtensions
{
    public static void AddAgents(this IServiceCollection services)
    {
        // Add the Semantic Kernel
        services.AddKernel();

        // Register the agents from our system
        services.AddKeyedSingleton(nameof(SentimentAgent), (sp, _) =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return SentimentAgent.CreateAgent(kernel);
        });
        services.AddKeyedSingleton(nameof(LanguageAgent), (sp, _) =>
        {
            var kernel = sp.GetRequiredService<Kernel>();
            return LanguageAgent.CreateAgent(kernel);
        });

        // Register the insights generator that will use the agents
        services.AddSingleton<Generator>();
    }
}
