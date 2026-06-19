using Microsoft.EntityFrameworkCore;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Initialization;

public sealed class DatabaseInitializer
{
    private readonly IDbContextFactory<ResearchWorkspaceDbContext>
        _dbContextFactory;

    public DatabaseInitializer(
        IDbContextFactory<ResearchWorkspaceDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        await dbContext.Database.MigrateAsync(
            cancellationToken);
    }
}