namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Models;

public sealed record PaperQueryRequest(
    string? SearchText,
    PaperSortOption SortOption,
    bool IncludeArchived,
    bool FavoritesOnly,
    int PageNumber,
    int PageSize);