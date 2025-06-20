using DataEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI.Embeddings;
using Products.Models; // Ensure Context is available

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(string search, Context db, EmbeddingClient embeddingClient,  int dimensions = 1536)
    {
        Console.WriteLine("Querying for similar products...");
        
        var embeddingSearch = embeddingClient.GenerateEmbedding(search, new() { Dimensions = dimensions });
        var vectorSearch = embeddingSearch.Value.ToFloats().ToArray();
        var relatedProducts = await db.Product
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vectorSearch))
            .Take(2)
            .ToListAsync();

        return Results.Ok(relatedProducts);
    }
}
