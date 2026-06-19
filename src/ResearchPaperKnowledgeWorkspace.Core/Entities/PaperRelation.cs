using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class PaperRelation : EntityBase
{
    public Guid SourcePaperId { get; set; }

    public Paper SourcePaper { get; set; } = null!;

    public Guid TargetPaperId { get; set; }

    public Paper TargetPaper { get; set; } = null!;

    public PaperRelationType RelationType { get; set; } =
        PaperRelationType.Related;

    public string? Description { get; set; }

    /// <summary>
    /// Confidence between 0 and 1.
    /// Manually created relations normally use 1.
    /// </summary>
    public double Confidence { get; set; } = 1.0;

    public bool IsUserConfirmed { get; set; } = true;
}