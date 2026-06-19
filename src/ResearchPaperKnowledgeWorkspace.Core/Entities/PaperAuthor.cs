using ResearchPaperKnowledgeWorkspace.Core.Common;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

/// <summary>
/// Explicit join entity between Paper and Author.
/// </summary>
public sealed class PaperAuthor : EntityBase
{
    public Guid PaperId { get; set; }

    public Paper Paper { get; set; } = null!;

    public Guid AuthorId { get; set; }

    public Author Author { get; set; } = null!;

    /// <summary>
    /// Zero-based or one-based ordering is allowed internally,
    /// but the application will standardize this later.
    /// </summary>
    public int AuthorOrder { get; set; }

    public bool IsCorrespondingAuthor { get; set; }

    public string? ContributionRole { get; set; }
}