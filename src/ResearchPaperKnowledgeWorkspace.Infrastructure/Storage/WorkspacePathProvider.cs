namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

public static class WorkspacePathProvider
{
    private const string ApplicationDirectoryName =
        "research-paper-and-knowledge-workspace";

    public static WorkspacePaths CreateDefault()
    {
        var localApplicationData = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localApplicationData))
        {
            throw new InvalidOperationException(
                "The local application data directory could not be resolved.");
        }

        var applicationDirectory = Path.Combine(
            localApplicationData,
            ApplicationDirectoryName);

        var dataDirectory = Path.Combine(
            applicationDirectory,
            "data");

        Directory.CreateDirectory(dataDirectory);

        var databasePath = Path.Combine(
            dataDirectory,
            "research-workspace.db");

        return new WorkspacePaths(
            applicationDirectory,
            dataDirectory,
            databasePath);
    }
}