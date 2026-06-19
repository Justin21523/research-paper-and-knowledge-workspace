using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class PaperTag : EntityBase
{
    public Guid PaperId { get; set; }

    public Paper Paper { get; set; } = null!;

    public Guid TagId { get; set; }

    public Tag Tag { get; set; } = null!;

    public TagAssignmentSource AssignmentSource { get; set; } =
        TagAssignmentSource.Manual;

    /// <summary>
    /// Confidence between 0 and 1 for automated suggestions.
    /// Manual assignments normally use 1.
    /// </summary>
    public double Confidence { get; set; } = 1.0;

    public DateTimeOffset AssignedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;
}