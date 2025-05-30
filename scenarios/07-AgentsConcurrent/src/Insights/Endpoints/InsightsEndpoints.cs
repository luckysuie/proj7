using DataEntities;
using Insights.Models;
using Microsoft.EntityFrameworkCore;

namespace Insights.Endpoints;

public static class InsightsEndpoints
{
   public static void MapInsightsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Insights");

        group.MapGet("/", async (Context db) =>
        {
            return await db.UserQuestionInsight.ToListAsync();
        })
        .WithName("GetAllInsights")
        .Produces<List<UserQuestionInsight>>(StatusCodes.Status200OK);

        routes.MapGet("/api/generateinsights/{userquestion}",
            async (string userquestion, Context db, Generator generator) =>
            {
                var result = await generator.GenerateInsightAsync(userquestion, db);

                return Results.Ok(result);
            })
            .WithName("AIGenerateInsight")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }
}
