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
        
        var filesDirectory = Path.Combine(
            applicationDirectory,
            "files");

        var temporaryImportDirectory = Path.Combine(
            applicationDirectory,
            "imports");

        Directory.CreateDirectory(filesDirectory);
        Directory.CreateDirectory(temporaryImportDirectory);

        return new WorkspacePaths(
            applicationDirectory,
            dataDirectory,
            databasePath,
            filesDirectory,
            temporaryImportDirectory);
    }
}