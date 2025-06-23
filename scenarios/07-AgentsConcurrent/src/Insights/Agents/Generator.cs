using DataEntities;
using Insights.Models;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;

#pragma warning disable SKEXP0001, SKEXP0110 

namespace Insights.Agents;

public class Generator(
    ILogger<Generator> logger,
    [FromKeyedServices(nameof(SentimentAgent))] Agent sentimentAgent,
    [FromKeyedServices(nameof(LanguageAgent))] Agent languageAgent)
{
    public async Task<string> GenerateInsightAsync(string search, Context db)
    {
        ConcurrentOrchestration orchestration = new(sentimentAgent, languageAgent);

        // Start the runtime
        InProcessRuntime runtime = new();
        await runtime.StartAsync();
        OrchestrationResult<string[]> output = await orchestration.InvokeAsync(search, runtime);

        // analyze the result of the concurrent agents run
        string[] analysisResults = await output.GetValueAsync(TimeSpan.FromSeconds(60));
        var analysisResult = TransformToAnalysis(analysisResults);
        await runtime.RunUntilIdleAsync();

        // add insight to the database
        var insight = new UserQuestionInsight
        {
            CreatedAt = DateTime.UtcNow,
            Question = search,
            Sentiment = analysisResult.Sentiment, 
            Language = analysisResult.Language
        };
        db.UserQuestionInsight.Add(insight);
        await db.SaveChangesAsync();
        var sanitizedQuestion = insight.Question.Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");
        logger.LogInformation("Added insight: {sanitizedQuestion}", sanitizedQuestion);
        return "OK";
    }

    public Analysis TransformToAnalysis(string[] messages)
    {
        Analysis analysisResult = new();
        
        foreach (var message in messages)
        {
            // analyze the message to see if it is a sentiment or language, and set the result into the analysisResult object
            if (message.Contains("sentiment", StringComparison.OrdinalIgnoreCase))
            {
                // Extract sentiment from the message
                var sentiment = message.Split(':')[1].Trim();
                analysisResult.Sentiment = Enum.TryParse<Sentiment>(sentiment, true, out var parsedSentiment) ? parsedSentiment : Sentiment.NotDefined;
            }
            else if (message.Contains("language", StringComparison.OrdinalIgnoreCase))
            {
                // Extract language from the message
                var language = message.Split(':')[1].Trim();
                analysisResult.Language = language;
            }
        }

        return analysisResult;
    }
}

public class Analysis
{
    public Sentiment Sentiment { get; set; } = Sentiment.NotDefined;
    public string Language { get; set; } = string.Empty;
}