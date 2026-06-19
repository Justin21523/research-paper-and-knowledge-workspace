using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Application.Common.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;

namespace ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;

public interface IPaperRepository
{
    Task<IReadOnlyList<Paper>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<Paper?> GetByIdAsync(
        Guid paperId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Paper paper,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(
        Paper paper,
        CancellationToken cancellationToken = default);
    Task<PagedResult<Paper>> QueryAsync(
        PaperQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SetFavoriteAsync(
        Guid paperId,
        bool isFavorite,
        CancellationToken cancellationToken = default);

    Task<bool> SetArchivedAsync(
        Guid paperId,
        bool isArchived,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid paperId,
        CancellationToken cancellationToken = default);
    Task<int> BatchSetFavoriteAsync(
        IReadOnlyCollection<Guid> paperIds,
        bool isFavorite,
        CancellationToken cancellationToken = default);

    Task<int> BatchSetArchivedAsync(
        IReadOnlyCollection<Guid> paperIds,
        bool isArchived,
        CancellationToken cancellationToken = default);

    Task<int> BatchDeleteAsync(
        IReadOnlyCollection<Guid> paperIds,
        CancellationToken cancellationToken = default);

    Task<int> BatchAssignTagAsync(
        IReadOnlyCollection<Guid> paperIds,
        Guid tagId,
        CancellationToken cancellationToken = default);

    Task<int> BatchAssignProjectAsync(
        IReadOnlyCollection<Guid> paperIds,
        Guid projectId,
        CancellationToken cancellationToken = default);

}