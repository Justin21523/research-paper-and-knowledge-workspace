using System;
using System.Threading;
using System.Threading.Tasks;
using ResearchPaperKnowledgeWorkspace.App.ViewModels.Imports;
using ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;
namespace ResearchPaperKnowledgeWorkspace.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(
        LibraryViewModel library,
        ImportWorkspaceViewModel imports)
    {
        Library = library;
        Imports = imports;

        Imports.ImportCompleted +=
            OnImportCompleted;
    }

    public LibraryViewModel Library { get; }
    public ImportWorkspaceViewModel Imports { get; }
    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        await Library.InitializeAsync(
            cancellationToken);

        await Imports.InitializeAsync(
            cancellationToken);
    }
    private async void OnImportCompleted(
        object? sender,
        EventArgs e)
    {
        await Library.ReloadAsync();
    }

}