using DataEntities;
using Insights.Models;
using OpenAI.Chat;

namespace Insights;

public class Generator
{
    private ILogger _logger;
    public ChatClient? _chatClient;

    public Generator(ILogger logger, ChatClient? chatClient)
    {
        _logger = logger;
        _chatClient = chatClient;
    }
    public async Task<string> GenerateInsightAsync(string search, Context db)
    {
        // add sample insight to the database
        var insight = new UserQuestionInsight
        {
            CreatedAt = DateTime.UtcNow,
            Question = search,
            Sentiment = Sentiment.Neutral,
            Language = "en"
        };
        db.UserQuestionInsight.Add(insight);
        await db.SaveChangesAsync();
        _logger.LogInformation($"Added insight: {insight.Question}");
        return "OK";
    }
}