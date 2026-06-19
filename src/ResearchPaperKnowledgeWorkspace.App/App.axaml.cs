using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

using ResearchPaperKnowledgeWorkspace.App.ViewModels;
using ResearchPaperKnowledgeWorkspace.App.Views;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Initialization;
using ResearchPaperKnowledgeWorkspace.Infrastructure.DependencyInjection;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;
using ResearchPaperKnowledgeWorkspace.Application.DependencyInjection;
using ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

namespace ResearchPaperKnowledgeWorkspace.App;

public partial class App : Avalonia.Application
{   
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is
            IClassicDesktopStyleApplicationLifetime desktop)
        {
            var workspacePaths =
                WorkspacePathProvider.CreateDefault();

            var services = new ServiceCollection();

            services.AddResearchWorkspaceApplication();
            services.AddResearchWorkspaceInfrastructure(
                workspacePaths);

            services.AddSingleton<PaperEditorViewModel>();
            services.AddSingleton<LibraryViewModel>();
            services.AddSingleton<PaperOrganizationViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            
            _serviceProvider = services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

            var databaseInitializer = _serviceProvider
                .GetRequiredService<DatabaseInitializer>();

            databaseInitializer
                .InitializeAsync()
                .GetAwaiter()
                .GetResult();

            var mainWindowViewModel = _serviceProvider
                .GetRequiredService<MainWindowViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            desktop.Exit += (_, _) =>
            {
                _serviceProvider?.Dispose();
                _serviceProvider = null;
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}