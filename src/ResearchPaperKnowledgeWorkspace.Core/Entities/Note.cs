using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class Note : EntityBase
{
    public Guid PaperId { get; set; }

    public Paper Paper { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string ContentMarkdown { get; set; } = string.Empty;

    public NoteType NoteType { get; set; } = NoteType.General;

    public bool IsPinned { get; set; }

    public DateTimeOffset LastEditedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;
}