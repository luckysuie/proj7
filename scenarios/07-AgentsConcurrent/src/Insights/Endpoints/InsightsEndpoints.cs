using DataEntities;
using Insights.Agents;
using Insights.Models;
using Microsoft.EntityFrameworkCore;

namespace Insights.Endpoints;

public static class InsightsEndpoints
{
   public static void MapInsightsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Insights");

        group
            .MapGet("/", static async (Context db) => await db.UserQuestionInsight.ToListAsync())
            .WithName("GetAllInsights")
            .Produces<List<UserQuestionInsight>>(StatusCodes.Status200OK);

        routes
            .MapGet("/api/generateinsights/{userquestion}", static async (string userquestion, Context db, Generator generator) =>
                {
                    var result = await generator.GenerateInsightAsync(userquestion, db);
                    return TypedResults.Ok(result);
                })
            .WithName("AIGenerateInsight")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }
}
