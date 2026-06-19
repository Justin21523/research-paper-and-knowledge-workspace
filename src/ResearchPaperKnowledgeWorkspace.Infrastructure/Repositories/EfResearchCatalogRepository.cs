using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Models;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Core.Enums;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Repositories;

public sealed class EfResearchCatalogRepository
    : IResearchCatalogRepository
{
    private readonly IDbContextFactory<ResearchWorkspaceDbContext>
        _dbContextFactory;

    public EfResearchCatalogRepository(
        IDbContextFactory<ResearchWorkspaceDbContext>
            dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<Author>> ListAuthorsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Authors
            .AsNoTracking()
            .OrderBy(author => author.SortName ?? author.FullName)
            .ThenBy(author => author.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> ListTagsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .ThenBy(tag => tag.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResearchProject>>
        ListProjectsAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.ResearchProjects
            .AsNoTracking()
            .Where(project => !project.IsArchived)
            .OrderBy(project => project.Name)
            .ThenBy(project => project.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAuthorAsync(
        Author author,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(author);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        dbContext.Authors.Add(author);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task AddTagAsync(
        Tag tag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tag);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        dbContext.Tags.Add(tag);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task AddProjectAsync(
        ResearchProject project,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(project);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        dbContext.ResearchProjects.Add(project);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<bool> TagNameExistsAsync(
        string normalizedName,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Tags.AnyAsync(
            tag => tag.NormalizedName == normalizedName,
            cancellationToken);
    }

    public async Task<bool> ProjectNameExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.ResearchProjects.AnyAsync(
            project =>
                project.Name.ToLower() == normalizedName,
            cancellationToken);
    }

    public async Task<bool> TagExistsAsync(
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.Tags.AnyAsync(
            tag => tag.Id == tagId,
            cancellationToken);
    }

    public async Task<bool> ProjectExistsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.ResearchProjects.AnyAsync(
            project => project.Id == projectId,
            cancellationToken);
    }

    public async Task<PaperOrganizationDetails?>
        GetPaperOrganizationAsync(
            Guid paperId,
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var paperExists = await dbContext.Papers.AnyAsync(
            paper => paper.Id == paperId,
            cancellationToken);

        if (!paperExists)
        {
            return null;
        }

        var authorIds = await dbContext.PaperAuthors
            .AsNoTracking()
            .Where(item => item.PaperId == paperId)
            .OrderBy(item => item.AuthorOrder)
            .Select(item => item.AuthorId)
            .ToListAsync(cancellationToken);

        var tagIds = await dbContext.PaperTags
            .AsNoTracking()
            .Where(item => item.PaperId == paperId)
            .OrderBy(item => item.Tag.Name)
            .Select(item => item.TagId)
            .ToListAsync(cancellationToken);

        var projectIds = await dbContext.ProjectPapers
            .AsNoTracking()
            .Where(item => item.PaperId == paperId)
            .OrderBy(item => item.ResearchProject.Name)
            .Select(item => item.ResearchProjectId)
            .ToListAsync(cancellationToken);

        return new PaperOrganizationDetails(
            paperId,
            authorIds,
            tagIds,
            projectIds);
    }

    public async Task<bool> ReplacePaperOrganizationAsync(
        UpdatePaperOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var paperExists = await dbContext.Papers.AnyAsync(
            paper => paper.Id == request.PaperId,
            cancellationToken);

        if (!paperExists)
        {
            return false;
        }

        var existingAuthors = await dbContext.PaperAuthors
            .Where(item => item.PaperId == request.PaperId)
            .ToListAsync(cancellationToken);

        var existingTags = await dbContext.PaperTags
            .Where(item => item.PaperId == request.PaperId)
            .ToListAsync(cancellationToken);

        var existingProjects = await dbContext.ProjectPapers
            .Where(item => item.PaperId == request.PaperId)
            .ToListAsync(cancellationToken);

        dbContext.PaperAuthors.RemoveRange(existingAuthors);
        dbContext.PaperTags.RemoveRange(existingTags);
        dbContext.ProjectPapers.RemoveRange(existingProjects);

        var authorIds = request.AuthorIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var tagIds = request.TagIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var projectIds = request.ProjectIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        for (var index = 0;
             index < authorIds.Length;
             index++)
        {
            dbContext.PaperAuthors.Add(
                new PaperAuthor
                {
                    PaperId = request.PaperId,
                    AuthorId = authorIds[index],
                    AuthorOrder = index + 1,
                    IsCorrespondingAuthor = index == 0
                });
        }

        foreach (var tagId in tagIds)
        {
            dbContext.PaperTags.Add(
                new PaperTag
                {
                    PaperId = request.PaperId,
                    TagId = tagId,
                    AssignmentSource =
                        TagAssignmentSource.Manual,
                    Confidence = 1
                });
        }

        foreach (var projectId in projectIds)
        {
            dbContext.ProjectPapers.Add(
                new ProjectPaper
                {
                    PaperId = request.PaperId,
                    ResearchProjectId = projectId,
                    SortOrder = 0
                });
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}