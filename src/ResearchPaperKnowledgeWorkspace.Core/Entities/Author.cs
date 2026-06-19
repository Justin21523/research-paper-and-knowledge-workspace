using ResearchPaperKnowledgeWorkspace.Core.Common;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

public sealed class Author : EntityBase
{
    /// <summary>
    /// Full display name. This is the primary required name field.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    public string? GivenName { get; set; }

    public string? FamilyName { get; set; }

    public string? SortName { get; set; }

    public string? Orcid { get; set; }

    public string? Affiliation { get; set; }

    public string? Biography { get; set; }

    public ICollection<PaperAuthor> PaperAuthors { get; set; } =
        new List<PaperAuthor>();
}