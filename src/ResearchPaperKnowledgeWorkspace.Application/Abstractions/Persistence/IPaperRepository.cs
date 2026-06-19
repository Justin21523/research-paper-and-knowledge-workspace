using ResearchPaperKnowledgeWorkspace.Core.Entities;

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
}