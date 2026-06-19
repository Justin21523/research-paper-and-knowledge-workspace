using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Models;

public sealed record PaperListItem(
    Guid Id,
    string Title,
    string AuthorsText,
    int? PublicationYear,
    string? JournalTitle,
    ReadingStatus ReadingStatus,
    bool IsFavorite,
    bool IsArchived,
    DateTimeOffset UpdatedAtUtc);