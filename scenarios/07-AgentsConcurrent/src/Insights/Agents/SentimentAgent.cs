using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace Insights.Agents;

public static class SentimentAgent
{
    private const string instructions = """
        You are an expert in sentiment analysis. Given a string, evaluate its sentiment and return one of the following values: positive, neutral, or negative. 
        The output should be in the format 'Sentiment:<detected sentiment>', in example: 'Sentiment:positive'
        """;

    private const string description = "An expert in evaluating and classifying the sentiment of text as positive, neutral, or negative.";

    private const string name = "SentimentAgent";

    public static Agent CreateAgent(Kernel kernel) => new ChatCompletionAgent
    {
        Instructions = instructions,
        Description = description,
        Name = name,
        Kernel = kernel,
    };
}
