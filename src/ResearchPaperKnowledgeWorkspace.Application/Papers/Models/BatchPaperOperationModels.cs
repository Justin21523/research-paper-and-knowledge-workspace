namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Models;

public sealed record BatchPaperOperationResult(
    int RequestedCount,
    int AffectedCount);

public sealed record BatchAssignTagRequest(
    IReadOnlyCollection<Guid> PaperIds,
    Guid TagId);

public sealed record BatchAssignProjectRequest(
    IReadOnlyCollection<Guid> PaperIds,
    Guid ProjectId);