using SearchEntities;

namespace Store.Services;

public interface IAzFunctionSearchService
{
    /// <summary>
    /// Performs a semantic search by calling the Azure Function endpoint.
    /// </summary>
    /// <param name="query">User query</param>
    /// <param name="top">Number of top results</param>
    /// <returns>SearchResponse or null</returns>
    Task<SearchResponse?> SearchAsync(string query, int top = 10);
}
