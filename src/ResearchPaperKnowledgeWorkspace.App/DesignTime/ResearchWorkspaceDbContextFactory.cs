using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.Sqlite;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

namespace ResearchPaperKnowledgeWorkspace.App.DesignTime;

/// <summary>
/// Creates the research workspace database context for EF Core tools.
/// </summary>
public sealed class ResearchWorkspaceDbContextFactory
    : IDesignTimeDbContextFactory<ResearchWorkspaceDbContext>
{
    public ResearchWorkspaceDbContext CreateDbContext(string[] args)
    {
        var workspacePaths =
            WorkspacePathProvider.CreateDefault();

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = workspacePaths.DatabasePath,
            ForeignKeys = true
        }.ToString();

        var options = new DbContextOptionsBuilder<
                ResearchWorkspaceDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new ResearchWorkspaceDbContext(options);
    }
}