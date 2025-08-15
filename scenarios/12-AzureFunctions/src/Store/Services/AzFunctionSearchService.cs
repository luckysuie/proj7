using System.Text.Json;
using System.Net.Http.Json;
using SearchEntities;
using Microsoft.Extensions.Configuration;

namespace Store.Services;

public class AzFunctionSearchService : IAzFunctionSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzFunctionSearchService> _logger;

    public AzFunctionSearchService(HttpClient httpClient, ILogger<AzFunctionSearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SearchResponse?> SearchAsync(string query, int top = 10)
    {
        try
        {
            Uri requestUri = new Uri(_httpClient.BaseAddress, $"api/semanticsearch");

            // complete the query to have the parameters query and top
            var queryParameters = new List<string>();
            if (!string.IsNullOrWhiteSpace(query))
            {
                queryParameters.Add($"query={Uri.EscapeDataString(query)}");
            }
            if (top > 0)
            {
                queryParameters.Add($"top={top}");
            }
            if (queryParameters.Count > 0)
            {
                requestUri = new Uri(requestUri, "?" + string.Join("&", queryParameters));
            }
            
            var response = await _httpClient.GetAsync(requestUri);
            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[AzFunctionSearchService] status: {Status}, content: {Content}", response.StatusCode, responseText);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content.ReadFromJsonAsync<SearchResponse>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize function response.");
                    return new SearchResponse { Response = responseText };
                }
            }

            return new SearchResponse { Response = responseText };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Azure Function semantic search.");
            return new SearchResponse { Response = "Error calling Azure Function" };
        }
    }
}
