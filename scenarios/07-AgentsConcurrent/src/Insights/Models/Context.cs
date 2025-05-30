using DataEntities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Insights.Models;

public class Context(DbContextOptions options) : DbContext(options)
{
    public DbSet<UserQuestionInsight> UserQuestionInsight => Set<UserQuestionInsight>();
}
