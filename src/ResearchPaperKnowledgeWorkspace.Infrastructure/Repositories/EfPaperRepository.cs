using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Application.Common.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

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
    public async Task UpdateAsync(
        Paper paper,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(paper);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        dbContext.Entry(paper).State =
            EntityState.Modified;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
    public async Task<PagedResult<Paper>> QueryAsync(
        PaperQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var query = dbContext.Papers
            .AsNoTracking()
            .Include(paper => paper.PaperAuthors)
            .ThenInclude(paperAuthor => paperAuthor.Author)
            .AsQueryable();

        if (!request.IncludeArchived)
        {
            query = query.Where(
                paper => !paper.IsArchived);
        }

        if (request.FavoritesOnly)
        {
            query = query.Where(
                paper => paper.IsFavorite);
        }

        var searchText =
            request.SearchText?.Trim().ToLower();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(
                paper =>
                    paper.Title.ToLower()
                        .Contains(searchText) ||
                    (paper.Subtitle != null &&
                    paper.Subtitle.ToLower()
                        .Contains(searchText)) ||
                    (paper.AbstractText != null &&
                    paper.AbstractText.ToLower()
                        .Contains(searchText)) ||
                    (paper.JournalTitle != null &&
                    paper.JournalTitle.ToLower()
                        .Contains(searchText)) ||
                    (paper.Doi != null &&
                    paper.Doi.ToLower()
                        .Contains(searchText)) ||
                    paper.PaperAuthors.Any(
                        paperAuthor =>
                            paperAuthor.Author.FullName
                                .ToLower()
                                .Contains(searchText)));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount /
                    (double)request.PageSize);

        var actualPage =
            totalPages == 0
                ? 1
                : Math.Clamp(
                    request.PageNumber,
                    1,
                    totalPages);

        query = ApplySorting(
            query,
            request.SortOption);

        var items = await query
            .Skip((actualPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Paper>(
            items,
            totalCount,
            actualPage,
            request.PageSize);
    }

    public async Task<bool> SetFavoriteAsync(
        Guid paperId,
        bool isFavorite,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var paper = await dbContext.Papers
            .SingleOrDefaultAsync(
                item => item.Id == paperId,
                cancellationToken);

        if (paper is null)
        {
            return false;
        }

        paper.IsFavorite = isFavorite;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> SetArchivedAsync(
        Guid paperId,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var paper = await dbContext.Papers
            .SingleOrDefaultAsync(
                item => item.Id == paperId,
                cancellationToken);

        if (paper is null)
        {
            return false;
        }

        paper.IsArchived = isArchived;

        if (isArchived)
        {
            paper.ReadingStatus =
                ReadingStatus.Archived;
        }
        else if (
            paper.ReadingStatus ==
            ReadingStatus.Archived)
        {
            paper.ReadingStatus =
                ReadingStatus.Unread;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid paperId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var paper = await dbContext.Papers
            .SingleOrDefaultAsync(
                item => item.Id == paperId,
                cancellationToken);

        if (paper is null)
        {
            return false;
        }

        var relations = await dbContext.PaperRelations
            .Where(
                relation =>
                    relation.SourcePaperId == paperId ||
                    relation.TargetPaperId == paperId)
            .ToListAsync(cancellationToken);

        dbContext.PaperRelations.RemoveRange(
            relations);

        dbContext.Papers.Remove(paper);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static IQueryable<Paper> ApplySorting(
        IQueryable<Paper> query,
        PaperSortOption sortOption)
    {
        return sortOption switch
        {
            PaperSortOption.UpdatedOldest =>
                query
                    .OrderBy(paper => paper.UpdatedAtUtc)
                    .ThenBy(paper => paper.Id),

            PaperSortOption.TitleAscending =>
                query
                    .OrderBy(paper => paper.Title)
                    .ThenBy(paper => paper.Id),

            PaperSortOption.TitleDescending =>
                query
                    .OrderByDescending(paper => paper.Title)
                    .ThenBy(paper => paper.Id),

            PaperSortOption.YearNewest =>
                query
                    .OrderBy(
                        paper =>
                            paper.PublicationYear == null)
                    .ThenByDescending(
                        paper =>
                            paper.PublicationYear)
                    .ThenBy(paper => paper.Id),

            PaperSortOption.YearOldest =>
                query
                    .OrderBy(
                        paper =>
                            paper.PublicationYear == null)
                    .ThenBy(
                        paper =>
                            paper.PublicationYear)
                    .ThenBy(paper => paper.Id),

            PaperSortOption.RatingHighest =>
                query
                    .OrderByDescending(
                        paper => paper.Rating)
                    .ThenByDescending(
                        paper => paper.UpdatedAtUtc)
                    .ThenBy(paper => paper.Id),

            PaperSortOption.PriorityHighest =>
                query
                    .OrderByDescending(
                        paper => paper.Priority)
                    .ThenByDescending(
                        paper => paper.UpdatedAtUtc)
                    .ThenBy(paper => paper.Id),

            _ =>
                query
                    .OrderByDescending(
                        paper => paper.UpdatedAtUtc)
                    .ThenBy(paper => paper.Id)
        };
    }

    public async Task<int> BatchSetFavoriteAsync(
        IReadOnlyCollection<Guid> paperIds,
        bool isFavorite,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeIds(paperIds);

        if (ids.Length == 0)
        {
            return 0;
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var papers = await dbContext.Papers
            .Where(paper => ids.Contains(paper.Id))
            .ToListAsync(cancellationToken);

        foreach (var paper in papers)
        {
            paper.IsFavorite = isFavorite;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return papers.Count;
    }

    public async Task<int> BatchSetArchivedAsync(
        IReadOnlyCollection<Guid> paperIds,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeIds(paperIds);

        if (ids.Length == 0)
        {
            return 0;
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var papers = await dbContext.Papers
            .Where(paper => ids.Contains(paper.Id))
            .ToListAsync(cancellationToken);

        foreach (var paper in papers)
        {
            paper.IsArchived = isArchived;

            if (isArchived)
            {
                paper.ReadingStatus =
                    ReadingStatus.Archived;
            }
            else if (
                paper.ReadingStatus ==
                ReadingStatus.Archived)
            {
                paper.ReadingStatus =
                    ReadingStatus.Unread;
            }
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return papers.Count;
    }

    public async Task<int> BatchDeleteAsync(
        IReadOnlyCollection<Guid> paperIds,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeIds(paperIds);

        if (ids.Length == 0)
        {
            return 0;
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var papers = await dbContext.Papers
            .Where(paper => ids.Contains(paper.Id))
            .ToListAsync(cancellationToken);

        var relations = await dbContext.PaperRelations
            .Where(
                relation =>
                    ids.Contains(relation.SourcePaperId) ||
                    ids.Contains(relation.TargetPaperId))
            .ToListAsync(cancellationToken);

        dbContext.PaperRelations.RemoveRange(relations);
        dbContext.Papers.RemoveRange(papers);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return papers.Count;
    }

    public async Task<int> BatchAssignTagAsync(
        IReadOnlyCollection<Guid> paperIds,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeIds(paperIds);

        if (ids.Length == 0 || tagId == Guid.Empty)
        {
            return 0;
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var validPaperIds = await dbContext.Papers
            .Where(paper => ids.Contains(paper.Id))
            .Select(paper => paper.Id)
            .ToListAsync(cancellationToken);

        var existingPaperIds = await dbContext.PaperTags
            .Where(
                item =>
                    item.TagId == tagId &&
                    validPaperIds.Contains(item.PaperId))
            .Select(item => item.PaperId)
            .ToListAsync(cancellationToken);

        var missingPaperIds = validPaperIds
            .Except(existingPaperIds)
            .ToArray();

        foreach (var paperId in missingPaperIds)
        {
            dbContext.PaperTags.Add(
                new PaperTag
                {
                    PaperId = paperId,
                    TagId = tagId,
                    AssignmentSource =
                        TagAssignmentSource.Manual,
                    Confidence = 1
                });
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return missingPaperIds.Length;
    }

    public async Task<int> BatchAssignProjectAsync(
        IReadOnlyCollection<Guid> paperIds,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var ids = NormalizeIds(paperIds);

        if (ids.Length == 0 || projectId == Guid.Empty)
        {
            return 0;
        }

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var validPaperIds = await dbContext.Papers
            .Where(paper => ids.Contains(paper.Id))
            .Select(paper => paper.Id)
            .ToListAsync(cancellationToken);

        var existingPaperIds = await dbContext.ProjectPapers
            .Where(
                item =>
                    item.ResearchProjectId == projectId &&
                    validPaperIds.Contains(item.PaperId))
            .Select(item => item.PaperId)
            .ToListAsync(cancellationToken);

        var missingPaperIds = validPaperIds
            .Except(existingPaperIds)
            .ToArray();

        foreach (var paperId in missingPaperIds)
        {
            dbContext.ProjectPapers.Add(
                new ProjectPaper
                {
                    PaperId = paperId,
                    ResearchProjectId = projectId,
                    SortOrder = 0
                });
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return missingPaperIds.Length;
    }

    private static Guid[] NormalizeIds(
        IReadOnlyCollection<Guid> paperIds)
    {
        ArgumentNullException.ThrowIfNull(paperIds);

        return paperIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
    }


}