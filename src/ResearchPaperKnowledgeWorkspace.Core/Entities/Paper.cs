using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Core.Entities;

/// <summary>
/// Represents an academic paper, report, thesis, book chapter,
/// conference paper, or another research resource.
/// </summary>
public sealed class Paper : EntityBase
{
    public string Title { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string? AbstractText { get; set; }

    public int? PublicationYear { get; set; }

    public DateTimeOffset? PublicationDate { get; set; }

    public string? JournalTitle { get; set; }

    public string? ConferenceName { get; set; }

    public string? Publisher { get; set; }

    public string? Volume { get; set; }

    public string? Issue { get; set; }

    public string? PageRange { get; set; }

    public string? Doi { get; set; }

    public string? Isbn { get; set; }

    public string? Issn { get; set; }

    public string? Url { get; set; }

    public string? LanguageCode { get; set; }

    public string? CitationKey { get; set; }

    public ReadingStatus ReadingStatus { get; set; } = ReadingStatus.Unread;

    /// <summary>
    /// Personal rating from 0 to 5.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Reading priority from 0 to 5.
    /// </summary>
    public int Priority { get; set; }

    public bool IsFavorite { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset? LastOpenedAtUtc { get; set; }

    public ICollection<PaperAuthor> PaperAuthors { get; set; } =
        new List<PaperAuthor>();

    public ICollection<PaperTag> PaperTags { get; set; } =
        new List<PaperTag>();

    public ICollection<ProjectPaper> ProjectPapers { get; set; } =
        new List<ProjectPaper>();

    public ICollection<Note> Notes { get; set; } =
        new List<Note>();

    public ICollection<Attachment> Attachments { get; set; } =
        new List<Attachment>();

    public ICollection<PaperRelation> OutgoingRelations { get; set; } =
        new List<PaperRelation>();

    public ICollection<PaperRelation> IncomingRelations { get; set; } =
        new List<PaperRelation>();
}