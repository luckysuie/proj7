using DataEntities;
using Microsoft.EntityFrameworkCore;

namespace Insights.Models;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<UserQuestionInsight> UserQuestionInsight => Set<UserQuestionInsight>();
}
