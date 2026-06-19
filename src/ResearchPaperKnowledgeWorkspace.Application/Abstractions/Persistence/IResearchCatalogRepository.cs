using ResearchPaperKnowledgeWorkspace.Application.Organization.Models;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;

public interface IResearchCatalogRepository
{
    Task<IReadOnlyList<Author>> ListAuthorsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tag>> ListTagsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResearchProject>> ListProjectsAsync(
        CancellationToken cancellationToken = default);

    Task AddAuthorAsync(
        Author author,
        CancellationToken cancellationToken = default);

    Task AddTagAsync(
        Tag tag,
        CancellationToken cancellationToken = default);

    Task AddProjectAsync(
        ResearchProject project,
        CancellationToken cancellationToken = default);

    Task<bool> TagNameExistsAsync(
        string normalizedName,
        CancellationToken cancellationToken = default);

    Task<bool> ProjectNameExistsAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> TagExistsAsync(
        Guid tagId,
        CancellationToken cancellationToken = default);

    Task<bool> ProjectExistsAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<PaperOrganizationDetails?> GetPaperOrganizationAsync(
        Guid paperId,
        CancellationToken cancellationToken = default);

    Task<bool> ReplacePaperOrganizationAsync(
        UpdatePaperOrganizationRequest request,
        CancellationToken cancellationToken = default);
}