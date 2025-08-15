using SearchEntities;
using DataEntities;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Store.Services;

public class ProductService
{
    HttpClient httpClient;
    private readonly ILogger<ProductService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IAzFunctionSearchService _azSearchService;

    public ProductService(HttpClient httpClient, ILogger<ProductService> logger, IConfiguration configuration, IAzFunctionSearchService azSearchService)
    {
        _logger = logger;
        this.httpClient = httpClient;
        _configuration = configuration;
        _azSearchService = azSearchService;
    }
    public async Task<List<Product>> GetProducts()
    {
        List<Product>? products = null;
        try
        {
            var response = await httpClient.GetAsync("/api/product");
            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Http status code: {response.StatusCode}");
            _logger.LogInformation($"Http response content: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                products = await response.Content.ReadFromJsonAsync(ProductSerializerContext.Default.ListProduct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GetProducts.");
        }

        return products ?? new List<Product>();
    }

    // Backwards compatible method
    public Task<SearchResponse?> Search(string searchTerm, bool semanticSearch = false)
        => Search(searchTerm, semanticSearch ? SearchMode.Semantic : SearchMode.Standard);

    public async Task<SearchResponse?> Search(string searchTerm, SearchMode mode)
    {
        try
        {
            // call the desired Endpoint
            HttpResponseMessage? response;
            switch (mode)
            {
                case SearchMode.Semantic:
                    // AI Search
                    response = await httpClient.GetAsync($"/api/aisearch/{searchTerm}");
                    break;
                case SearchMode.AzureFunctionSemantic:
                    {
                        // Delegate to the AzFunctionSearchService which will use service discovery or configured endpoint
                        var azResult = await _azSearchService.SearchAsync(searchTerm, 10);
                        return azResult ?? new SearchResponse { Response = "No response from Azure Function service" };
                    }
                default:
                    // standard search
                    response = await httpClient.GetAsync($"/api/product/search/{searchTerm}");
                    break;
            }

            if (response == null)
            {
                return new SearchResponse { Response = "No response" };
            }

            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Http status code: {response.StatusCode}");
            _logger.LogInformation($"Http response content: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                // Try to read as SearchResponse
                try
                {
                    return await response.Content.ReadFromJsonAsync<SearchResponse>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize response into SearchResponse. Returning raw content.");
                    return new SearchResponse { Response = responseText };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Search.");
        }

        return new SearchResponse { Response = "No response" };
    }
}
