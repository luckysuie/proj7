using McpToolsEntities;
using ModelContextProtocol.Server;
using Services;
using System.ComponentModel;

namespace McpSample.AspNetCoreSseServer;

[McpServerToolType]
public static class WeatherTool
{
    [McpServerTool(Name = "GetWeatherForCity"), 
        Description("Returns the current weather forecast for a city.")]
    public static async Task<WeatherToolResponse> GetWeatherForCity(
        WeatherService weatherService,
        ILogger<ProductService> logger,
        IMcpServer currentMcpServer,
        [Description("The name of the city to get the weather information")] string cityName)
    {
        Console.WriteLine("==========================");
        Console.WriteLine($"Function Start WeatherTool: GetWeatherForCity called with cityName: {cityName}");

        var response = await weatherService.GetWeather(cityName);

        Console.WriteLine($"Function End WeatherTool");
        Console.WriteLine("==========================");

        return response;        
    }
}
