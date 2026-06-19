namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Models;

public sealed record CreatePaperRequest(
    string Title,
    int? PublicationYear = null,
    string? JournalTitle = null,
    string? Doi = null,
    string? AbstractText = null);