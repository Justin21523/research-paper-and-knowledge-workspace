namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

/// <summary>
/// Contains the application storage paths used by the workspace.
/// </summary>
public sealed record WorkspacePaths(
    string ApplicationDirectory,
    string DataDirectory,
    string DatabasePath);