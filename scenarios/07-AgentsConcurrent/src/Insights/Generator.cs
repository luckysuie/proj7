using DataEntities;
using Insights.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Orchestration.Transforms;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;

#pragma warning disable SKEXP0110 

namespace Insights;

public class Generator
{
    private ILogger _logger;
    public ChatClient? _chatClient;
    private Kernel _kernel;

    public Generator(ILogger logger, ChatClient? chatClient, Kernel kernel)
    {
        _logger = logger;
        _chatClient = chatClient;
        _kernel = kernel;
    }
    public async Task<string> GenerateInsightAsync(string search, Context db)
    {

        // Define the agents
        ChatCompletionAgent agentSentiment =
            CreateAgent(
                instructions: "You are an expert in sentiment analysis. Given a string, evaluate its sentiment and return one of the following values: positive, neutral, or negative.",
                description: "An expert in evaluating and classifying the sentiment of text as positive, neutral, or negative.");
        ChatCompletionAgent agentLanguage =
            this.CreateAgent(
                instructions: "You are an expert in language detection. Given a string, detect the language and return its standard language code (e.g., en for English, es for Spanish, fr for French, etc.).",
                description: "An expert in detecting the language of text and returning its language code.");


        // Define the orchestration with transform
        StructuredOutputTransform<Analysis> outputTransform = new(_kernel.GetRequiredService<IChatCompletionService>(),
                new OpenAIPromptExecutionSettings { ResponseFormat = typeof(Analysis) });

        ConcurrentOrchestration<string, Analysis> orchestration =
            new(agentSentiment, agentLanguage)
            {
                ResultTransform = outputTransform.TransformAsync,
            };

        // Start the runtime
        InProcessRuntime runtime = new();
        await runtime.StartAsync();
        OrchestrationResult<Analysis> result = await orchestration.InvokeAsync(search, runtime);

        Analysis output = await result.GetValueAsync();
        
        await runtime.RunUntilIdleAsync();


        // add sample insight to the database
        var insight = new UserQuestionInsight
        {
            CreatedAt = DateTime.UtcNow,
            Question = search,
            Sentiment = output.Sentiment, // Sentiment.Neutral,
            Language = output.Language // "en"
        };
        db.UserQuestionInsight.Add(insight);
        await db.SaveChangesAsync();
        _logger.LogInformation($"Added insight: {insight.Question}");
        return "OK";
    }

    protected ChatCompletionAgent CreateAgent(string instructions, string? description = null, string? name = null)
    {
        return
            new ChatCompletionAgent
            {
                Name = name,
                Description = description,
                Instructions = instructions,
                Kernel = _kernel
            };
    }
    
    protected void LogInsight(UserQuestionInsight insight)
    {
        _logger.LogInformation($"Added insight: {insight.Question} with Sentiment: {insight.Sentiment} and Language: {insight.Language}");
    }   
}

public class Analysis
{
    public Sentiment Sentiment { get; set; } = Sentiment.NotDefined;
    public string Language { get; set; } = string.Empty;
}