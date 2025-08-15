using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace SemanticSearchFunction.Services;

public static class AzureOpenAiEmbeddingProvider
{
    // Added static factory method for creating an embedding generator from the configured OpenAI connection string
    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingClient(IConfiguration configuration, string deploymentName)
    {
        if (string.IsNullOrWhiteSpace(deploymentName))
            throw new ArgumentException("Deployment name not configured", nameof(deploymentName));

        var conn = configuration.GetConnectionString("openai");
        if (string.IsNullOrWhiteSpace(conn))
            throw new InvalidOperationException("Connection string 'openai' not found. Expected format: Endpoint=https://<resource>.openai.azure.com/models/;Key=<api key>;");

        // Parse connection string (Endpoint=...;Key=...;)
        var parts = conn.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (kv.Length == 2) dict[kv[0]] = kv[1];
        }

        if (!dict.TryGetValue("Endpoint", out var endpoint) || string.IsNullOrWhiteSpace(endpoint))
            throw new InvalidOperationException("'Endpoint' not found in 'openai' connection string");
        if (!dict.TryGetValue("Key", out var key) || string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("'Key' not found in 'openai' connection string");

        // Normalize endpoint (remove trailing /models/ or /models)
        endpoint = endpoint.TrimEnd('/');
        if (endpoint.EndsWith("/models", StringComparison.OrdinalIgnoreCase))
            endpoint = endpoint[..^7]; // remove '/models'


        AzureOpenAIClient client = null;

        if (!string.IsNullOrWhiteSpace(key))
        {
            client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        }
        else
        {
            client = new AzureOpenAIClient(new Uri(endpoint), new Azure.Identity.DefaultAzureCredential());
        }

        // Microsoft.Extensions.AI provides extension to adapt AzureOpenAIClient to IEmbeddingGenerator
        var embeddingClient = client.GetEmbeddingClient(deploymentName);

        return embeddingClient.AsIEmbeddingGenerator();
    }
}
