using ResearchPaperKnowledgeWorkspace.Core.Common;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class Tag : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public string? NormalizedName { get; set; }

    public string? Description { get; set; }

    public string? ColorHex { get; set; }

    public Guid? ParentTagId { get; set; }

    public Tag? ParentTag { get; set; }

    public ICollection<Tag> ChildTags { get; set; } =
        new List<Tag>();

    public ICollection<PaperTag> PaperTags { get; set; } =
        new List<PaperTag>();
}