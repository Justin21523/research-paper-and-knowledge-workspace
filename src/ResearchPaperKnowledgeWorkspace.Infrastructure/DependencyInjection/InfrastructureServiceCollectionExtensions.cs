using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Initialization;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Repositories;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Development;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Seeding;

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
            
        services.AddSingleton<IPaperRepository, EfPaperRepository>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<
            IDemoDataSeeder,
            DatabaseDemoDataSeeder>();
        services.AddSingleton<
            IResearchCatalogRepository,
            EfResearchCatalogRepository>();
            
        return services;
    }
}