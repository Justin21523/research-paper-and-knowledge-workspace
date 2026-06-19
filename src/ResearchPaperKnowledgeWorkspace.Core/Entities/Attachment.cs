using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class Attachment : EntityBase
{
    public Guid PaperId { get; set; }

    public Paper Paper { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string? MimeType { get; set; }

    public string? FileExtension { get; set; }

    public long FileSizeBytes { get; set; }

    public string? Sha256Hash { get; set; }

    public AttachmentType AttachmentType { get; set; } =
        AttachmentType.Unknown;

    public bool IsPrimary { get; set; }

    public bool IsFileAvailable { get; set; } = true;

    public DateTimeOffset ImportedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;
}