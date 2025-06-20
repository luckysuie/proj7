using Microsoft.EntityFrameworkCore;
using OpenAI.Embeddings;
using Products.Models; // Ensure Context is available

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(string search, Context db, EmbeddingClient embeddingClient)
    {
        Console.WriteLine("Querying for similar products...");
        float[] vector = embeddingClient.GenerateEmbedding(search).Value.ToFloats().ToArray();
        var relatedProducts = await db.Product
                .Where(p => p.Id > 0)
                .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vector))
                .Select(p => p)
                .Take(3)
                .ToListAsync();

        return Results.Ok(relatedProducts);
    }
}
