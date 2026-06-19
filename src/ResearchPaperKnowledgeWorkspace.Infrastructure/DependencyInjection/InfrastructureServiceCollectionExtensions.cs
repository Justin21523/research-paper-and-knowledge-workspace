using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Initialization;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddResearchWorkspaceInfrastructure(
        this IServiceCollection services,
        WorkspacePaths workspacePaths)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(workspacePaths);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = workspacePaths.DatabasePath,
            ForeignKeys = true
        }.ToString();

        services.AddSingleton(workspacePaths);

        services.AddDbContextFactory<ResearchWorkspaceDbContext>(
            options =>
            {
                options.UseSqlite(connectionString);
            });

        services.AddSingleton<DatabaseInitializer>();

        return services;
    }
}