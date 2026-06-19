using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Models;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Services;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

public sealed class PaperOrganizationViewModel
    : ViewModelBase
{
    private readonly PaperOrganizationService
        _organizationService;

    private readonly HashSet<Guid> _originalAuthorIds = [];
    private readonly HashSet<Guid> _originalTagIds = [];
    private readonly HashSet<Guid> _originalProjectIds = [];

    private bool _catalogLoaded;
    private bool _isApplying;
    private Guid _paperId;

    private string _newAuthorName = string.Empty;
    private string _newAuthorOrcid = string.Empty;
    private string _newAuthorAffiliation = string.Empty;

    private string _newTagName = string.Empty;
    private string _newTagDescription = string.Empty;
    private string _newTagColorHex = string.Empty;

    private string _newProjectName = string.Empty;
    private string _newProjectDescription = string.Empty;
    private string _newProjectColorHex = string.Empty;
    private ProjectStatus _newProjectStatus =
        ProjectStatus.Active;

    private bool _isDirty;
    private bool _isBusy;
    private string? _errorMessage;
    private string? _statusMessage;

    public PaperOrganizationViewModel(
        PaperOrganizationService organizationService)
    {
        _organizationService = organizationService;

        SaveCommand = new AsyncRelayCommand(
            SaveAsync,
            CanSave);

        RevertCommand = new RelayCommand(
            Revert,
            CanRevert);

        RefreshCatalogCommand = new AsyncRelayCommand(
            RefreshCatalogAsync,
            CanRunCatalogCommand);

        CreateAuthorCommand = new AsyncRelayCommand(
            CreateAuthorAsync,
            CanCreateAuthor);

        CreateTagCommand = new AsyncRelayCommand(
            CreateTagAsync,
            CanCreateTag);

        CreateProjectCommand = new AsyncRelayCommand(
            CreateProjectAsync,
            CanCreateProject);
    }

    public event EventHandler<Guid>? Saved;

    public event EventHandler? CatalogChanged;

    public ObservableCollection<
        AuthorSelectionItemViewModel> Authors { get; } = [];

    public ObservableCollection<
        TagSelectionItemViewModel> Tags { get; } = [];

    public ObservableCollection<
        ProjectSelectionItemViewModel> Projects { get; } = [];

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand RevertCommand { get; }

    public IAsyncRelayCommand RefreshCatalogCommand { get; }

    public IAsyncRelayCommand CreateAuthorCommand { get; }

    public IAsyncRelayCommand CreateTagCommand { get; }

    public IAsyncRelayCommand CreateProjectCommand { get; }

    public IReadOnlyList<ProjectStatus>
        ProjectStatusOptions { get; } =
            Enum.GetValues<ProjectStatus>();

    public bool HasPaper =>
        PaperId != Guid.Empty;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasStatusMessage =>
        !string.IsNullOrWhiteSpace(StatusMessage);

    public string DirtyStateText =>
        IsDirty
            ? "Unsaved organization changes"
            : "Organization changes saved";

    public string AuthorCountText =>
        $"{Authors.Count(item => item.IsSelected)} selected";

    public string TagCountText =>
        $"{Tags.Count(item => item.IsSelected)} selected";

    public string ProjectCountText =>
        $"{Projects.Count(item => item.IsSelected)} selected";

    public Guid PaperId
    {
        get => _paperId;

        private set
        {
            if (SetProperty(ref _paperId, value))
            {
                OnPropertyChanged(nameof(HasPaper));
                NotifyCommandStates();
            }
        }
    }

    public bool IsDirty
    {
        get => _isDirty;

        private set
        {
            if (!SetProperty(ref _isDirty, value))
            {
                return;
            }

            OnPropertyChanged(nameof(DirtyStateText));
            NotifyCommandStates();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;

        private set
        {
            if (!SetProperty(ref _isBusy, value))
            {
                return;
            }

            NotifyCommandStates();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;

        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public string? StatusMessage
    {
        get => _statusMessage;

        private set
        {
            if (SetProperty(ref _statusMessage, value))
            {
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public string NewAuthorName
    {
        get => _newAuthorName;

        set
        {
            if (SetProperty(
                    ref _newAuthorName,
                    value ?? string.Empty))
            {
                CreateAuthorCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NewAuthorOrcid
    {
        get => _newAuthorOrcid;

        set => SetProperty(
            ref _newAuthorOrcid,
            value ?? string.Empty);
    }

    public string NewAuthorAffiliation
    {
        get => _newAuthorAffiliation;

        set => SetProperty(
            ref _newAuthorAffiliation,
            value ?? string.Empty);
    }

    public string NewTagName
    {
        get => _newTagName;

        set
        {
            if (SetProperty(
                    ref _newTagName,
                    value ?? string.Empty))
            {
                CreateTagCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NewTagDescription
    {
        get => _newTagDescription;

        set => SetProperty(
            ref _newTagDescription,
            value ?? string.Empty);
    }

    public string NewTagColorHex
    {
        get => _newTagColorHex;

        set => SetProperty(
            ref _newTagColorHex,
            value ?? string.Empty);
    }

    public string NewProjectName
    {
        get => _newProjectName;

        set
        {
            if (SetProperty(
                    ref _newProjectName,
                    value ?? string.Empty))
            {
                CreateProjectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NewProjectDescription
    {
        get => _newProjectDescription;

        set => SetProperty(
            ref _newProjectDescription,
            value ?? string.Empty);
    }

    public string NewProjectColorHex
    {
        get => _newProjectColorHex;

        set => SetProperty(
            ref _newProjectColorHex,
            value ?? string.Empty);
    }

    public ProjectStatus NewProjectStatus
    {
        get => _newProjectStatus;

        set => SetProperty(
            ref _newProjectStatus,
            value);
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (_catalogLoaded)
        {
            return;
        }

        await LoadCatalogAsync(cancellationToken);
    }

    public async Task LoadPaperAsync(
        Guid paperId,
        CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var organization =
            await _organizationService
                .GetPaperOrganizationAsync(
                    paperId,
                    cancellationToken);

        _isApplying = true;

        try
        {
            PaperId = paperId;

            ApplySelections(
                organization.AuthorIds,
                organization.TagIds,
                organization.ProjectIds);

            CaptureOriginalSelections();
        }
        finally
        {
            _isApplying = false;
        }

        IsDirty = false;
        ErrorMessage = null;
        StatusMessage = null;
    }

    public void ClearPaper()
    {
        _isApplying = true;

        try
        {
            PaperId = Guid.Empty;
            ApplySelections([], [], []);

            _originalAuthorIds.Clear();
            _originalTagIds.Clear();
            _originalProjectIds.Clear();
        }
        finally
        {
            _isApplying = false;
        }

        IsDirty = false;
        ErrorMessage = null;
        StatusMessage = null;
    }

    private async Task LoadCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        var selectedAuthors = SelectedAuthorIds();
        var selectedTags = SelectedTagIds();
        var selectedProjects = SelectedProjectIds();

        var catalog =
            await _organizationService.GetCatalogAsync(
                cancellationToken);

        DetachSelectionEvents();

        Authors.Clear();
        Tags.Clear();
        Projects.Clear();

        foreach (var author in catalog.Authors)
        {
            var item =
                new AuthorSelectionItemViewModel(author);

            item.SelectionChanged +=
                OnCatalogSelectionChanged;

            item.SetSelectedSilently(
                selectedAuthors.Contains(author.Id));

            Authors.Add(item);
        }

        foreach (var tag in catalog.Tags)
        {
            var item =
                new TagSelectionItemViewModel(tag);

            item.SelectionChanged +=
                OnCatalogSelectionChanged;

            item.SetSelectedSilently(
                selectedTags.Contains(tag.Id));

            Tags.Add(item);
        }

        foreach (var project in catalog.Projects)
        {
            var item =
                new ProjectSelectionItemViewModel(project);

            item.SelectionChanged +=
                OnCatalogSelectionChanged;

            item.SetSelectedSilently(
                selectedProjects.Contains(project.Id));

            Projects.Add(item);
        }

        _catalogLoaded = true;

        NotifySelectionCounts();

        CatalogChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task RefreshCatalogAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await LoadCatalogAsync();

            StatusMessage =
                "Research catalog refreshed.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to refresh catalog: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        if (!HasPaper)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            await _organizationService
                .UpdatePaperOrganizationAsync(
                    new UpdatePaperOrganizationRequest(
                        PaperId,
                        SelectedAuthorIds(),
                        SelectedTagIds(),
                        SelectedProjectIds()));

            CaptureOriginalSelections();

            IsDirty = false;

            StatusMessage =
                "Paper organization saved successfully.";

            Saved?.Invoke(this, PaperId);
        }
        catch (RequestValidationException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (EntityNotFoundException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to save organization: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Revert()
    {
        _isApplying = true;

        try
        {
            ApplySelections(
                _originalAuthorIds,
                _originalTagIds,
                _originalProjectIds);
        }
        finally
        {
            _isApplying = false;
        }

        IsDirty = false;
        ErrorMessage = null;
        StatusMessage =
            "Unsaved organization changes were reverted.";
    }

    private async Task CreateAuthorAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await _organizationService.CreateAuthorAsync(
                new CreateAuthorRequest(
                    NewAuthorName,
                    Orcid: NewAuthorOrcid,
                    Affiliation: NewAuthorAffiliation));

            NewAuthorName = string.Empty;
            NewAuthorOrcid = string.Empty;
            NewAuthorAffiliation = string.Empty;

            await LoadCatalogAsync();

            StatusMessage =
                "Author created successfully.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to create author: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateTagAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await _organizationService.CreateTagAsync(
                new CreateTagRequest(
                    NewTagName,
                    NewTagDescription,
                    NewTagColorHex));

            NewTagName = string.Empty;
            NewTagDescription = string.Empty;
            NewTagColorHex = string.Empty;

            await LoadCatalogAsync();

            StatusMessage =
                "Tag created successfully.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to create tag: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateProjectAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await _organizationService.CreateProjectAsync(
                new CreateResearchProjectRequest(
                    NewProjectName,
                    NewProjectDescription,
                    NewProjectStatus,
                    NewProjectColorHex));

            NewProjectName = string.Empty;
            NewProjectDescription = string.Empty;
            NewProjectColorHex = string.Empty;
            NewProjectStatus = ProjectStatus.Active;

            await LoadCatalogAsync();

            StatusMessage =
                "Research project created successfully.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to create project: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCatalogSelectionChanged(
        object? sender,
        EventArgs e)
    {
        if (_isApplying)
        {
            return;
        }

        NotifySelectionCounts();

        IsDirty =
            !SelectedAuthorIds().ToHashSet()
                .SetEquals(_originalAuthorIds) ||
            !SelectedTagIds().ToHashSet()
                .SetEquals(_originalTagIds) ||
            !SelectedProjectIds().ToHashSet()
                .SetEquals(_originalProjectIds);
    }

    private void ApplySelections(
        IEnumerable<Guid> authorIds,
        IEnumerable<Guid> tagIds,
        IEnumerable<Guid> projectIds)
    {
        var authorSet = authorIds.ToHashSet();
        var tagSet = tagIds.ToHashSet();
        var projectSet = projectIds.ToHashSet();

        foreach (var author in Authors)
        {
            author.SetSelectedSilently(
                authorSet.Contains(author.Id));
        }

        foreach (var tag in Tags)
        {
            tag.SetSelectedSilently(
                tagSet.Contains(tag.Id));
        }

        foreach (var project in Projects)
        {
            project.SetSelectedSilently(
                projectSet.Contains(project.Id));
        }

        NotifySelectionCounts();
    }

    private void CaptureOriginalSelections()
    {
        ReplaceSet(
            _originalAuthorIds,
            SelectedAuthorIds());

        ReplaceSet(
            _originalTagIds,
            SelectedTagIds());

        ReplaceSet(
            _originalProjectIds,
            SelectedProjectIds());
    }

    private Guid[] SelectedAuthorIds()
    {
        return Authors
            .Where(item => item.IsSelected)
            .Select(item => item.Id)
            .ToArray();
    }

    private Guid[] SelectedTagIds()
    {
        return Tags
            .Where(item => item.IsSelected)
            .Select(item => item.Id)
            .ToArray();
    }

    private Guid[] SelectedProjectIds()
    {
        return Projects
            .Where(item => item.IsSelected)
            .Select(item => item.Id)
            .ToArray();
    }

    private static void ReplaceSet(
        HashSet<Guid> destination,
        IEnumerable<Guid> values)
    {
        destination.Clear();

        foreach (var value in values)
        {
            destination.Add(value);
        }
    }

    private void DetachSelectionEvents()
    {
        foreach (var item in Authors)
        {
            item.SelectionChanged -=
                OnCatalogSelectionChanged;
        }

        foreach (var item in Tags)
        {
            item.SelectionChanged -=
                OnCatalogSelectionChanged;
        }

        foreach (var item in Projects)
        {
            item.SelectionChanged -=
                OnCatalogSelectionChanged;
        }
    }

    private bool CanSave()
    {
        return HasPaper &&
               IsDirty &&
               !IsBusy;
    }

    private bool CanRevert()
    {
        return HasPaper &&
               IsDirty &&
               !IsBusy;
    }

    private bool CanRunCatalogCommand()
    {
        return !IsBusy;
    }

    private bool CanCreateAuthor()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(NewAuthorName);
    }

    private bool CanCreateTag()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(NewTagName);
    }

    private bool CanCreateProject()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(NewProjectName);
    }

    private void NotifySelectionCounts()
    {
        OnPropertyChanged(nameof(AuthorCountText));
        OnPropertyChanged(nameof(TagCountText));
        OnPropertyChanged(nameof(ProjectCountText));
    }

    private void NotifyCommandStates()
    {
        SaveCommand.NotifyCanExecuteChanged();
        RevertCommand.NotifyCanExecuteChanged();
        RefreshCatalogCommand.NotifyCanExecuteChanged();
        CreateAuthorCommand.NotifyCanExecuteChanged();
        CreateTagCommand.NotifyCanExecuteChanged();
        CreateProjectCommand.NotifyCanExecuteChanged();
    }
}