using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class ImportJob : EntityBase
{
    public string OriginalFilePath { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public ImportJobStatus Status { get; set; } =
        ImportJobStatus.Pending;

    public AttachmentType AttachmentType { get; set; } =
        AttachmentType.Unknown;

    public string? Sha256Hash { get; set; }

    public string? DetectedTitle { get; set; }

    public string? DetectedAuthorText { get; set; }

    public string? ErrorMessage { get; set; }

    public Guid? PaperId { get; set; }

    public Guid? AttachmentId { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }
}