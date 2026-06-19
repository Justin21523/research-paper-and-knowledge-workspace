using System;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Models;

public sealed record PaperDetails(
    Guid Id,
    string Title,
    string? Subtitle,
    string AuthorsText,
    string? AbstractText,
    int? PublicationYear,
    string? JournalTitle,
    string? ConferenceName,
    string? Publisher,
    string? Volume,
    string? Issue,
    string? PageRange,
    string? Doi,
    string? Isbn,
    string? Issn,
    string? Url,
    string? LanguageCode,
    string? CitationKey,
    ReadingStatus ReadingStatus,
    int Rating,
    int Priority,
    bool IsFavorite,
    DateTimeOffset UpdatedAtUtc);