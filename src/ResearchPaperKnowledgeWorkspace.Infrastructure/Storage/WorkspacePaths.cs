/// <summary>
/// Contains the application storage paths used by the workspace.
/// </summary>
namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

public sealed record WorkspacePaths(
    string ApplicationDirectory,
    string DataDirectory,
    string DatabasePath,
    string FilesDirectory,
    string TemporaryImportDirectory);