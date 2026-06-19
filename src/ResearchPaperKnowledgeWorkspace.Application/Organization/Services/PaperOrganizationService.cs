using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Application.Organization.Services;

public sealed class PaperOrganizationService
{
    private readonly IResearchCatalogRepository
        _catalogRepository;

    private readonly IPaperRepository
        _paperRepository;

    public PaperOrganizationService(
        IResearchCatalogRepository catalogRepository,
        IPaperRepository paperRepository)
    {
        _catalogRepository = catalogRepository;
        _paperRepository = paperRepository;
    }

    public async Task<ResearchCatalog> GetCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var authors = await _catalogRepository.ListAuthorsAsync(
            cancellationToken);

        var tags = await _catalogRepository.ListTagsAsync(
            cancellationToken);

        var projects =
            await _catalogRepository.ListProjectsAsync(
                cancellationToken);

        return new ResearchCatalog(
            authors.Select(MapAuthor).ToList(),
            tags.Select(MapTag).ToList(),
            projects.Select(MapProject).ToList());
    }

    public async Task<Guid> CreateAuthorAsync(
        CreateAuthorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var fullName = RequireText(
            request.FullName,
            "Author name is required.");

        var author = new Author
        {
            FullName = fullName,
            GivenName = OptionalText(request.GivenName),
            FamilyName = OptionalText(request.FamilyName),
            SortName = fullName,
            Orcid = OptionalText(request.Orcid),
            Affiliation = OptionalText(request.Affiliation)
        };

        await _catalogRepository.AddAuthorAsync(
            author,
            cancellationToken);

        return author.Id;
    }

    public async Task<Guid> CreateTagAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = RequireText(
            request.Name,
            "Tag name is required.");

        var normalizedName =
            name.ToLowerInvariant();

        if (await _catalogRepository.TagNameExistsAsync(
                normalizedName,
                cancellationToken))
        {
            throw new RequestValidationException(
                "A tag with the same name already exists.");
        }

        var tag = new Tag
        {
            Name = name,
            NormalizedName = normalizedName,
            Description = OptionalText(
                request.Description),
            ColorHex = OptionalText(request.ColorHex)
        };

        await _catalogRepository.AddTagAsync(
            tag,
            cancellationToken);

        return tag.Id;
    }

    public async Task<Guid> CreateProjectAsync(
        CreateResearchProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = RequireText(
            request.Name,
            "Project name is required.");

        if (await _catalogRepository.ProjectNameExistsAsync(
                name,
                cancellationToken))
        {
            throw new RequestValidationException(
                "A project with the same name already exists.");
        }

        var project = new ResearchProject
        {
            Name = name,
            Description = OptionalText(
                request.Description),
            Status = request.Status,
            ColorHex = OptionalText(request.ColorHex),
            StartedAtUtc = DateTimeOffset.UtcNow
        };

        await _catalogRepository.AddProjectAsync(
            project,
            cancellationToken);

        return project.Id;
    }

    public async Task<PaperOrganizationDetails>
        GetPaperOrganizationAsync(
            Guid paperId,
            CancellationToken cancellationToken = default)
    {
        if (paperId == Guid.Empty)
        {
            throw new RequestValidationException(
                "A valid paper identifier is required.");
        }

        return await _catalogRepository
            .GetPaperOrganizationAsync(
                paperId,
                cancellationToken)
            ?? throw new EntityNotFoundException(
                "The selected paper could not be found.");
    }

    public async Task UpdatePaperOrganizationAsync(
        UpdatePaperOrganizationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.PaperId == Guid.Empty)
        {
            throw new RequestValidationException(
                "A valid paper identifier is required.");
        }

        var catalog = await GetCatalogAsync(
            cancellationToken);

        ValidateIdentifiers(
            request.AuthorIds,
            catalog.Authors.Select(item => item.Id),
            "One or more selected authors no longer exist.");

        ValidateIdentifiers(
            request.TagIds,
            catalog.Tags.Select(item => item.Id),
            "One or more selected tags no longer exist.");

        ValidateIdentifiers(
            request.ProjectIds,
            catalog.Projects.Select(item => item.Id),
            "One or more selected projects no longer exist.");

        if (!await _catalogRepository
                .ReplacePaperOrganizationAsync(
                    request,
                    cancellationToken))
        {
            throw new EntityNotFoundException(
                "The selected paper could not be found.");
        }
    }

    public Task<BatchPaperOperationResult>
        SetBatchFavoriteAsync(
            IReadOnlyCollection<Guid> paperIds,
            bool isFavorite,
            CancellationToken cancellationToken = default)
    {
        return ExecuteBatchAsync(
            paperIds,
            ids => _paperRepository.BatchSetFavoriteAsync(
                ids,
                isFavorite,
                cancellationToken));
    }

    public Task<BatchPaperOperationResult>
        SetBatchArchivedAsync(
            IReadOnlyCollection<Guid> paperIds,
            bool isArchived,
            CancellationToken cancellationToken = default)
    {
        return ExecuteBatchAsync(
            paperIds,
            ids => _paperRepository.BatchSetArchivedAsync(
                ids,
                isArchived,
                cancellationToken));
    }

    public Task<BatchPaperOperationResult>
        DeleteBatchAsync(
            IReadOnlyCollection<Guid> paperIds,
            CancellationToken cancellationToken = default)
    {
        return ExecuteBatchAsync(
            paperIds,
            ids => _paperRepository.BatchDeleteAsync(
                ids,
                cancellationToken));
    }

    public async Task<BatchPaperOperationResult>
        AssignTagToBatchAsync(
            BatchAssignTagRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _catalogRepository.TagExistsAsync(
                request.TagId,
                cancellationToken))
        {
            throw new EntityNotFoundException(
                "The selected tag could not be found.");
        }

        return await ExecuteBatchAsync(
            request.PaperIds,
            ids => _paperRepository.BatchAssignTagAsync(
                ids,
                request.TagId,
                cancellationToken));
    }

    public async Task<BatchPaperOperationResult>
        AssignProjectToBatchAsync(
            BatchAssignProjectRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await _catalogRepository.ProjectExistsAsync(
                request.ProjectId,
                cancellationToken))
        {
            throw new EntityNotFoundException(
                "The selected project could not be found.");
        }

        return await ExecuteBatchAsync(
            request.PaperIds,
            ids => _paperRepository.BatchAssignProjectAsync(
                ids,
                request.ProjectId,
                cancellationToken));
    }

    private static async Task<BatchPaperOperationResult>
        ExecuteBatchAsync(
            IReadOnlyCollection<Guid> paperIds,
            Func<IReadOnlyCollection<Guid>, Task<int>>
                operation)
    {
        ArgumentNullException.ThrowIfNull(paperIds);

        var ids = paperIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            throw new RequestValidationException(
                "Select at least one paper.");
        }

        var affectedCount =
            await operation(ids);

        return new BatchPaperOperationResult(
            ids.Length,
            affectedCount);
    }

    private static void ValidateIdentifiers(
        IEnumerable<Guid> selectedIds,
        IEnumerable<Guid> availableIds,
        string errorMessage)
    {
        var availableSet =
            availableIds.ToHashSet();

        if (selectedIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Any(id => !availableSet.Contains(id)))
        {
            throw new RequestValidationException(
                errorMessage);
        }
    }

    private static AuthorCatalogItem MapAuthor(
        Author author)
    {
        return new AuthorCatalogItem(
            author.Id,
            author.FullName,
            author.Orcid,
            author.Affiliation);
    }

    private static TagCatalogItem MapTag(Tag tag)
    {
        return new TagCatalogItem(
            tag.Id,
            tag.Name,
            tag.Description,
            tag.ColorHex);
    }

    private static ResearchProjectCatalogItem MapProject(
        ResearchProject project)
    {
        return new ResearchProjectCatalogItem(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.ColorHex);
    }

    private static string RequireText(
        string? value,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RequestValidationException(
                errorMessage);
        }

        return value.Trim();
    }

    private static string? OptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}