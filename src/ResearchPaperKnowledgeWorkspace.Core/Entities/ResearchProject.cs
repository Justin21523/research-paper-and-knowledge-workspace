using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class ResearchProject : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

    public string? ColorHex { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? TargetCompletionAtUtc { get; set; }

    public bool IsArchived { get; set; }

    public ICollection<ProjectPaper> ProjectPapers { get; set; } =
        new List<ProjectPaper>();
}