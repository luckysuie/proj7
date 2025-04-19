using McpToolsEntities;
using System.Globalization;

namespace ParkInformationAgent.EndPoints;

public static class ParkInformationAgentEndPoints
{
    public static void MapParkInformationEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        routes.MapGet("/", () => $"Park Information Agent - {DateTime.Now}").ExcludeFromDescription();

        routes.MapGet("/getparkinfo/{parkName}",
            async (string parkName,
            ILogger <Program> logger,
            IConfiguration config) =>
            {
                return await GetParkInformationAsync(parkName, logger, config);
            })
            .WithName("GetParkInfo")
            .Produces<ParkInformationToolResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    internal static async Task<ParkInformationToolResponse> GetParkInformationAsync(
        string parkName, ILogger<Program> logger, IConfiguration config)
    {
        ParkInformationToolResponse response = new ParkInformationToolResponse
        {
            ParkName = parkName
        };
        return response;
    }

}
