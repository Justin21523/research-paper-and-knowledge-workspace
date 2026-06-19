using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Repositories;

public sealed class EfPaperRepository : IPaperRepository
{
    private readonly IDbContextFactory<ResearchWorkspaceDbContext>
        _dbContextFactory;

    public EfPaperRepository(
        IDbContextFactory<ResearchWorkspaceDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<Paper>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Papers
            .AsNoTracking()
            .Include(paper => paper.PaperAuthors)
            .ThenInclude(paperAuthor => paperAuthor.Author)
            .OrderByDescending(paper => paper.UpdatedAtUtc)
            .ThenBy(paper => paper.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Paper?> GetByIdAsync(
        Guid paperId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Papers
            .AsNoTracking()
            .Include(paper => paper.PaperAuthors)
            .ThenInclude(paperAuthor => paperAuthor.Author)
            .SingleOrDefaultAsync(
                paper => paper.Id == paperId,
                cancellationToken);
    }

    public async Task AddAsync(
        Paper paper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paper);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        dbContext.Papers.Add(paper);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}