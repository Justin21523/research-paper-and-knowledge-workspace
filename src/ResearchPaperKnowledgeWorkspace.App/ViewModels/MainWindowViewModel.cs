using System.Threading;
using System.Threading.Tasks;
using ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(
        LibraryViewModel library)
    {
        Library = library;
    }

    public LibraryViewModel Library { get; }

    public Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        return Library.InitializeAsync(cancellationToken);
    }
}