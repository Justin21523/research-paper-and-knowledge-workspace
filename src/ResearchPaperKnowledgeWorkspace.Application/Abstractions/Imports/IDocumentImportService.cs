using ResearchPaperKnowledgeWorkspace.Application.Imports.Models;

namespace ResearchPaperKnowledgeWorkspace.Application.Abstractions.Imports;

public interface IDocumentImportService
{
    Task<ImportBatchResult> ImportAsync(
        IReadOnlyCollection<string> sourcePaths,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImportQueueItem>> GetRecentJobsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);
    Task<ImportFileResult> RetryAsync(
        Guid importJobId,
        CancellationToken cancellationToken = default);

    Task<int> ClearCompletedJobsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttachmentListItem>>
        GetPaperAttachmentsAsync(
            Guid paperId,
            CancellationToken cancellationToken = default);


}