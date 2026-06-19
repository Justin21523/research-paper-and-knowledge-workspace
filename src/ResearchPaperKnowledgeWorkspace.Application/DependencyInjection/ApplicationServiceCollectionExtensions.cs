using Microsoft.Extensions.DependencyInjection;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Services;

namespace ResearchPaperKnowledgeWorkspace.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddResearchWorkspaceApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<PaperLibraryService>();
        services.AddSingleton<PaperOrganizationService>();
        
        return services;
    }
}