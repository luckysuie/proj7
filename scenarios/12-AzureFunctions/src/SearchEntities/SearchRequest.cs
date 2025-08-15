using System.Text.Json.Serialization;

namespace SearchEntities;

public class SearchRequest
{
    public SearchRequest()
    {
    }

    [JsonPropertyName("query")]
    public string? query { get; set; }

    [JsonPropertyName("top")]
    public int? top { get; set; }

}


[JsonSerializable(typeof(SearchRequest))]
public sealed partial class SearchRequestSerializerContext : JsonSerializerContext
{
}