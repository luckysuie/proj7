using McpToolsEntities;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SearchEntities;
using Services;
using System.Text;

namespace Store.Services;

public class McpServerService
{
    private readonly ILogger<ProductService> logger;
    IMcpClient mcpClient = null!;
    IList<McpClientTool> tools = null!;
    private Microsoft.Extensions.AI.IChatClient? chatClient;
    private IList<ChatMessage> ChatMessages = [];

    public McpServerService(ILogger<ProductService> _logger, IMcpClient _mcpClient, IChatClient? _chatClient)
    {
        logger = _logger;
        mcpClient = _mcpClient;
        chatClient = _chatClient;

        // get mcp server tools
        tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
    }

    public IList<McpClientTool> GetTools()
    {
        return tools;
    }

    public async Task<SearchResponse?> Search(string searchTerm,
        IList<McpClientTool>? tools = null,
        IList<McpClientTool>? selectedTools = null)
    {
        try
        {
            // init chat messages
            var systemMessage = ""; // CreateSystemMessage(tools, selectedTools);
            ChatMessages = [];
            ChatMessages.Add(new ChatMessage(ChatRole.System, systemMessage));

            ChatOptions chatOptions = new ChatOptions
            {
                Tools = [.. selectedTools]
            };


            // call the desired Endpoint
            ChatMessages.Add(new ChatMessage(ChatRole.User, searchTerm));
            var responseComplete = await chatClient.GetResponseAsync(
                ChatMessages,
                chatOptions);
            logger.LogInformation($"Model Response: {responseComplete}");
            ChatMessages.AddMessages(responseComplete);

            // create search response
            SearchResponse searchResponse = new SearchResponse
            {
                Response = responseComplete.Text
            };


            // iterate through the messages
            foreach (var message in responseComplete.Messages)
            {
                // validate if the message is a function call
                if (message.Role == ChatRole.Tool)
                {
                    var functionResult = message.Contents.FirstOrDefault() as FunctionResultContent;
                    string functionResultJsonString = functionResult.Result.ToString();

                    // from the functionResultJson, get the element at [JSON].content.[0].text
                    // this is the serialization from the function call response object
                    var functionResultJson = System.Text.Json.JsonDocument.Parse(functionResultJsonString);
                    var searchResponseJson = functionResultJson.RootElement.GetProperty("content").EnumerateArray().FirstOrDefault().GetProperty("text").ToString();


                    // try to deserialize the message.RawRepresentation, in Json, to a SearchResponse object
                    try
                    {
                        var deserializedToolResponse = DeserializeResponseJson(searchResponseJson);
                        deserializedToolResponse.ToolCallId = functionResult.CallId;

                        searchResponse.McpFunctionCallId = deserializedToolResponse.ToolCallId;
                        if (string.IsNullOrEmpty(searchResponse.McpFunctionCallName))
                        { 
                            searchResponse.McpFunctionCallName = deserializedToolResponse.ToolName; 
                        }

                        if (deserializedToolResponse is ProductsSearchToolResponse productsSearchToolResponse)
                        {
                            searchResponse.McpFunctionCallName = productsSearchToolResponse.SearchResponse.McpFunctionCallName;
                            searchResponse.Products = productsSearchToolResponse.SearchResponse.Products;
                            searchResponse.Response = productsSearchToolResponse.SearchResponse.Response;
                        }
                        else if (deserializedToolResponse is WeatherToolResponse weatherToolResponse)
                        {
                            searchResponse.Response = weatherToolResponse.WeatherCondition;
                        }
                        else if (deserializedToolResponse is ParkInformationToolResponse parkInformationToolResponse)
                        {
                            searchResponse.Response = parkInformationToolResponse.ParkInformation;
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.LogError(exc, "Error deserializing function result JSON to SearchResponse object.");
                    }
                }
            }


            return searchResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Search.");
        }

        return new SearchResponse { Response = "No response" };
    }

    private ToolResponse? DeserializeResponseJson(string json)
    {
        try
        {
            // First try to determine the type of response based on the JSON structure
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
            var rootElement = jsonDoc.RootElement;

            // Check for WeatherResponse properties
            if (rootElement.TryGetProperty("CityName", out _) ||
                rootElement.TryGetProperty("WeatherCondition", out _))
            {
                var weatherResponse = System.Text.Json.JsonSerializer.Deserialize<WeatherToolResponse>(json);
                logger.LogInformation($"Deserialized JSON as WeatherResponse: City={weatherResponse?.CityName}, Condition={weatherResponse?.WeatherCondition}");
                return weatherResponse;
            }

            // Check for ParkInformationResponse properties
            if (rootElement.TryGetProperty("ParkName", out _) ||
                rootElement.TryGetProperty("ParkInformation", out _))
            {
                var parkResponse = System.Text.Json.JsonSerializer.Deserialize<ParkInformationToolResponse>(json);
                logger.LogInformation($"Deserialized JSON as ParkInformationResponse: Park={parkResponse?.ParkName}, Information={parkResponse?.ParkInformation}");
                return parkResponse;
            }

            // Check for SearchResponse properties (Products, Response, etc.)
            if (rootElement.TryGetProperty("Products", out _) ||
                rootElement.TryGetProperty("McpFunctionCallName", out _))
            {
                var searchResponse = System.Text.Json.JsonSerializer.Deserialize<ProductsSearchToolResponse>(json);
                logger.LogInformation($"Deserialized JSON as SearchResponse: Products count={searchResponse?.SearchResponse.Products?.Count ?? 0}");
                return searchResponse;
            }

            // Default to SearchResponse if no specific type was detected
            logger.LogWarning("Could not determine specific response type, defaulting to SearchResponse");
            return System.Text.Json.JsonSerializer.Deserialize<ProductsSearchToolResponse>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error deserializing JSON");
            return null;
        }
    }

    private string CreateSystemMessage(IList<McpClientTool> allTools, IList<McpClientTool> selectedTools)
    {
        // Start with the original system message
        StringBuilder message = new("You are a helpful assistant. You always replies using text and emojis. You only do what the user ask you to do. If you don't have a function or a tool to answer a question, you just answer the question.");

        message.AppendLine();
        message.AppendLine();
        message.AppendLine("# Tool Usage Instructions");

        // Add information about all tools and which ones are selected
        if (allTools != null && allTools.Any())
        {
            message.AppendLine("You have access to the following tools, but you must ONLY use the tools that are explicitly marked as 'ALLOWED':");
            message.AppendLine();

            foreach (var tool in allTools)
            {
                bool isSelected = selectedTools != null && selectedTools.Any(t => t.Name == tool.Name);

                message.AppendLine($"- {tool.Name}");
                message.AppendLine($"  Status: {(isSelected ? "ALLOWED" : "NOT ALLOWED")}");
                if (!isSelected)
                {
                    message.AppendLine($"  DO NOT USE the {tool.Name} tool even if it seems appropriate for the query.");
                }
                message.AppendLine();
            }

            message.AppendLine("Important: Only use tools marked as ALLOWED. If none of the allowed tools can help with the user's request, respond using only your knowledge without calling any tools.");
        }


        return message.ToString();
    }
}