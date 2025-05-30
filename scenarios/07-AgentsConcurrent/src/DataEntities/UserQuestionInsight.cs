using System.Text.Json.Serialization;

namespace DataEntities;

public enum Sentiment
{
    [JsonPropertyName("positive")]
    Positive,
    [JsonPropertyName("neutral")]
    Neutral,
    [JsonPropertyName("negative")]
    Negative
}

public class UserQuestionInsight
{
    [JsonPropertyName("id")]
    public int Id { get; set; } // For auto-increment in DB

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } // Creation date of the query

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("sentiment")]
    public Sentiment Sentiment { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;
}
