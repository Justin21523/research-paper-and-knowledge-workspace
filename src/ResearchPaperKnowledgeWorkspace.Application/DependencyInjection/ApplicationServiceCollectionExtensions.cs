using Microsoft.Extensions.DependencyInjection;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;

namespace ResearchPaperKnowledgeWorkspace.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddResearchWorkspaceApplication(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<PaperLibraryService>();

        return services;
    }
}