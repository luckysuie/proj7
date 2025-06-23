using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace Insights.Agents;

public static class LanguageAgent
{
    private const string instructions = """
        You are an expert in language detection. Given a string, detect the language and return its standard language code (e.g., en for English, es for Spanish, fr for French, etc.).
        The output should be in the format 'Language:<detected language>', in example: 'Language:en'
        """;
    private const string description = "An expert in detecting the language of text and returning its language code.";
    private const string name = "LanguageAgent";
    public static Agent CreateAgent(Kernel kernel) => new ChatCompletionAgent
    {
        Instructions = instructions,
        Description = description,
        Name = name,
        Kernel = kernel,
    };
}