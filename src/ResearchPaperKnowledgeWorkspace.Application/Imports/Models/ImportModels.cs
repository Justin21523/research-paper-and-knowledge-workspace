using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Application.Imports.Models;

public sealed record ImportFileResult(
    Guid ImportJobId,
    string FileName,
    ImportJobStatus Status,
    Guid? PaperId,
    Guid? AttachmentId,
    string? ErrorMessage);

public sealed record ImportBatchResult(
    IReadOnlyList<ImportFileResult> Items)
{
    public int TotalCount => Items.Count;

    public int SucceededCount =>
        Items.Count(item =>
            item.Status == ImportJobStatus.Succeeded);

    public int DuplicateCount =>
        Items.Count(item =>
            item.Status == ImportJobStatus.Duplicate);

    public int FailedCount =>
        Items.Count(item =>
            item.Status == ImportJobStatus.Failed);
}

public sealed record ImportQueueItem(
    Guid Id,
    string FileName,
    ImportJobStatus Status,
    AttachmentType AttachmentType,
    string? DetectedTitle,
    string? ErrorMessage,
    Guid? PaperId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);