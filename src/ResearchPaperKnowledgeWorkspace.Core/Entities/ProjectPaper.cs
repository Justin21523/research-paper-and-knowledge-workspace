using ResearchPaperKnowledgeWorkspace.Core.Common;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class ProjectPaper : EntityBase
{
    public Guid ResearchProjectId { get; set; }

    public ResearchProject ResearchProject { get; set; } = null!;

    public Guid PaperId { get; set; }

    public Paper Paper { get; set; } = null!;

    public DateTimeOffset AddedAtUtc { get; set; } =
        DateTimeOffset.UtcNow;

    public int SortOrder { get; set; }

    public string? ProjectSpecificNote { get; set; }
}