using Microsoft.EntityFrameworkCore;
using OpenAI.Embeddings;
using Products.Models; // Ensure Context is available

namespace Products.Endpoints;

public static class ProductAiActions
{
    public static async Task<IResult> AISearch(string search, Context db, EmbeddingClient embeddingClient)
    {
        //var result = await mc.Search(search, db);


        Console.WriteLine("Querying for similar posts...");
        float[] vector = embeddingClient.GenerateEmbedding(search).Value.ToFloats().ToArray();
        var relatedProducts = await db.Product
            .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vector))
            .Select(p => new { p.Name, Distance = EF.Functions.VectorDistance("cosine", p.Embedding, vector) })
            .Take(3)
            .ToListAsync();

        return Results.Ok(relatedProducts);
    }
}
