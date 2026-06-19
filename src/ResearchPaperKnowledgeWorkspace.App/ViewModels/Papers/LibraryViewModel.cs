using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ResearchPaperKnowledgeWorkspace.App.ViewModels;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Development;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

public sealed class LibraryViewModel : ViewModelBase
{
    private readonly PaperLibraryService _paperLibraryService;
    private readonly IDemoDataSeeder _demoDataSeeder;

    private bool _isInitialized;
    private CancellationTokenSource?
        _detailsCancellationTokenSource;

    private string _titleInput = string.Empty;
    private string _publicationYearInput = string.Empty;
    private string _journalTitleInput = string.Empty;
    private string _doiInput = string.Empty;

    private string _searchText = string.Empty;
    private PaperSortOption _selectedSortOption =
        PaperSortOption.UpdatedNewest;

    private bool _includeArchived;
    private bool _favoritesOnly;

    private int _currentPage = 1;
    private int _pageSize = 25;
    private int _totalCount;
    private int _totalPages;

    private string? _errorMessage;
    private string? _statusMessage;

    private bool _isLoading;
    private bool _isCreating;
    private bool _isUpdatingPaper;
    private bool _isSeeding;
    private bool _isDeleteConfirmationVisible;

    private PaperListItem? _selectedPaper;
    private PaperDetails? _selectedPaperDetails;
    private bool _isLoadingDetails;

    public LibraryViewModel(
        PaperLibraryService paperLibraryService,
        PaperEditorViewModel editor,
        IDemoDataSeeder demoDataSeeder)
    {
        _paperLibraryService = paperLibraryService;
        _demoDataSeeder = demoDataSeeder;

        Editor = editor;
        Editor.Saved += OnEditorSaved;

        RefreshCommand = new AsyncRelayCommand(
            RefreshAsync,
            CanRunGeneralCommand);

        CreatePaperCommand = new AsyncRelayCommand(
            CreatePaperAsync,
            CanCreatePaper);

        SearchCommand = new AsyncRelayCommand(
            SearchAsync,
            CanRunGeneralCommand);

        ClearSearchCommand = new AsyncRelayCommand(
            ClearSearchAsync,
            CanRunGeneralCommand);

        PreviousPageCommand = new AsyncRelayCommand(
            PreviousPageAsync,
            CanGoToPreviousPage);

        NextPageCommand = new AsyncRelayCommand(
            NextPageAsync,
            CanGoToNextPage);

        ToggleFavoriteCommand = new AsyncRelayCommand(
            ToggleFavoriteAsync,
            CanModifySelectedPaper);

        ToggleArchiveCommand = new AsyncRelayCommand(
            ToggleArchiveAsync,
            CanModifySelectedPaper);

        RequestDeleteCommand = new RelayCommand(
            RequestDelete,
            CanModifySelectedPaper);

        CancelDeleteCommand = new RelayCommand(
            CancelDelete);

        ConfirmDeleteCommand = new AsyncRelayCommand(
            ConfirmDeleteAsync,
            CanConfirmDelete);

        SeedDemoDataCommand = new AsyncRelayCommand(
            SeedDemoDataAsync,
            CanRunGeneralCommand);
    }

    public ObservableCollection<PaperListItem> Papers { get; } =
        new();

    public PaperEditorViewModel Editor { get; }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand CreatePaperCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand ClearSearchCommand { get; }
    public IAsyncRelayCommand PreviousPageCommand { get; }
    public IAsyncRelayCommand NextPageCommand { get; }
    public IAsyncRelayCommand ToggleFavoriteCommand { get; }
    public IAsyncRelayCommand ToggleArchiveCommand { get; }
    public IRelayCommand RequestDeleteCommand { get; }
    public IRelayCommand CancelDeleteCommand { get; }
    public IAsyncRelayCommand ConfirmDeleteCommand { get; }
    public IAsyncRelayCommand SeedDemoDataCommand { get; }

    public IReadOnlyList<PaperSortOption> SortOptions { get; } =
        Enum.GetValues<PaperSortOption>();

    public IReadOnlyList<int> PageSizeOptions { get; } =
        new[] { 10, 25, 50, 100 };

    public bool HasPapers =>
        Papers.Count > 0;

    public bool IsEmpty =>
        !IsLoading &&
        Papers.Count == 0;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasStatusMessage =>
        !string.IsNullOrWhiteSpace(StatusMessage);

    public bool IsBusy =>
        IsLoading ||
        IsCreating ||
        IsUpdatingPaper ||
        IsSeeding;

    public string PaperCountText =>
        TotalCount == 1
            ? "1 paper"
            : $"{TotalCount} papers";

    public string PageStatusText =>
        TotalCount == 0
            ? "No results"
            : $"Page {CurrentPage} of {TotalPages} · {TotalCount} papers";

    public bool CanGoPrevious =>
        CurrentPage > 1;

    public bool CanGoNext =>
        TotalPages > 0 &&
        CurrentPage < TotalPages;

    public bool HasSelectedPaper =>
        SelectedPaper is not null;

    public bool HasNoSelectedPaper =>
        SelectedPaper is null;

    public bool HasSelectedPaperDetails =>
        SelectedPaperDetails is not null &&
        !IsLoadingDetails;

    public string FavoriteActionText =>
        SelectedPaper?.IsFavorite == true
            ? "Remove Favorite"
            : "Add Favorite";

    public string ArchiveActionText =>
        SelectedPaper?.IsArchived == true
            ? "Restore"
            : "Archive";

    public string DeleteConfirmationText =>
        SelectedPaper is null
            ? string.Empty
            : $"Delete “{SelectedPaper.Title}” permanently?";

    public string TitleInput
    {
        get => _titleInput;
        set
        {
            if (SetProperty(
                    ref _titleInput,
                    value ?? string.Empty))
            {
                NotifyCommandStates();
            }
        }
    }

    public string PublicationYearInput
    {
        get => _publicationYearInput;
        set => SetProperty(
            ref _publicationYearInput,
            value ?? string.Empty);
    }

    public string JournalTitleInput
    {
        get => _journalTitleInput;
        set => SetProperty(
            ref _journalTitleInput,
            value ?? string.Empty);
    }

    public string DoiInput
    {
        get => _doiInput;
        set => SetProperty(
            ref _doiInput,
            value ?? string.Empty);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(
            ref _searchText,
            value ?? string.Empty);
    }

    public PaperSortOption SelectedSortOption
    {
        get => _selectedSortOption;
        set => SetProperty(
            ref _selectedSortOption,
            value);
    }

    public bool IncludeArchived
    {
        get => _includeArchived;
        set => SetProperty(
            ref _includeArchived,
            value);
    }

    public bool FavoritesOnly
    {
        get => _favoritesOnly;
        set => SetProperty(
            ref _favoritesOnly,
            value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                NotifyPagingStateChanged();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                CurrentPage = 1;
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set
        {
            if (SetProperty(ref _totalCount, value))
            {
                OnPropertyChanged(nameof(PaperCountText));
                OnPropertyChanged(nameof(PageStatusText));
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            if (SetProperty(ref _totalPages, value))
            {
                NotifyPagingStateChanged();
            }
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

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                NotifyBusyStateChanged();
            }
        }
    }

    public bool IsCreating
    {
        get => _isCreating;
        private set
        {
            if (SetProperty(ref _isCreating, value))
            {
                NotifyBusyStateChanged();
            }
        }
    }

    public bool IsUpdatingPaper
    {
        get => _isUpdatingPaper;
        private set
        {
            if (SetProperty(ref _isUpdatingPaper, value))
            {
                NotifyBusyStateChanged();
            }
        }
    }

    public bool IsSeeding
    {
        get => _isSeeding;
        private set
        {
            if (SetProperty(ref _isSeeding, value))
            {
                NotifyBusyStateChanged();
            }
        }
    }

    public bool IsDeleteConfirmationVisible
    {
        get => _isDeleteConfirmationVisible;
        private set
        {
            if (SetProperty(
                    ref _isDeleteConfirmationVisible,
                    value))
            {
                NotifyCommandStates();
            }
        }
    }

    public PaperListItem? SelectedPaper
    {
        get => _selectedPaper;
        set
        {
            if (!SetProperty(ref _selectedPaper, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasSelectedPaper));
            OnPropertyChanged(nameof(HasNoSelectedPaper));
            OnPropertyChanged(nameof(FavoriteActionText));
            OnPropertyChanged(nameof(ArchiveActionText));
            OnPropertyChanged(nameof(DeleteConfirmationText));

            HandleSelectedPaperChanged(value);
            NotifyCommandStates();
        }
    }

    public PaperDetails? SelectedPaperDetails
    {
        get => _selectedPaperDetails;
        private set
        {
            if (SetProperty(
                    ref _selectedPaperDetails,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasSelectedPaperDetails));
            }
        }
    }

    public bool IsLoadingDetails
    {
        get => _isLoadingDetails;
        private set
        {
            if (SetProperty(ref _isLoadingDetails, value))
            {
                OnPropertyChanged(
                    nameof(HasSelectedPaperDetails));
            }
        }
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        await LoadPapersAsync(cancellationToken);
    }

    private Task RefreshAsync()
    {
        return LoadPapersAsync();
    }

    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadPapersAsync();
    }

    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        FavoritesOnly = false;
        IncludeArchived = false;
        SelectedSortOption =
            PaperSortOption.UpdatedNewest;
        CurrentPage = 1;

        await LoadPapersAsync();
    }

    private async Task PreviousPageAsync()
    {
        if (!CanGoPrevious)
        {
            return;
        }

        CurrentPage--;
        await LoadPapersAsync();
    }

    private async Task NextPageAsync()
    {
        if (!CanGoNext)
        {
            return;
        }

        CurrentPage++;
        await LoadPapersAsync();
    }

    private async Task CreatePaperAsync()
    {
        IsCreating = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var publicationYear =
                ParsePublicationYear(
                    PublicationYearInput);

            var paperId =
                await _paperLibraryService
                    .CreatePaperAsync(
                        new CreatePaperRequest(
                            Title: TitleInput,
                            PublicationYear:
                                publicationYear,
                            JournalTitle:
                                JournalTitleInput,
                            Doi: DoiInput));

            ClearQuickAddForm();

            CurrentPage = 1;

            await ReloadAndSelectAsync(
                paperId);

            StatusMessage =
                "Paper added successfully.";
        }
        catch (RequestValidationException exception)
        {
            ErrorMessage = exception.Message;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to add the paper: {exception.Message}";
        }
        finally
        {
            IsCreating = false;
        }
    }

    private async Task ToggleFavoriteAsync()
    {
        var paper = SelectedPaper;

        if (paper is null)
        {
            return;
        }

        IsUpdatingPaper = true;

        try
        {
            await _paperLibraryService.SetFavoriteAsync(
                paper.Id,
                !paper.IsFavorite);

            await ReloadAndSelectAsync(paper.Id);

            StatusMessage =
                "Favorite state updated.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to update favorite state: {exception.Message}";
        }
        finally
        {
            IsUpdatingPaper = false;
        }
    }

    private async Task ToggleArchiveAsync()
    {
        var paper = SelectedPaper;

        if (paper is null)
        {
            return;
        }

        IsUpdatingPaper = true;

        try
        {
            await _paperLibraryService.SetArchivedAsync(
                paper.Id,
                !paper.IsArchived);

            await ReloadAndSelectAsync(paper.Id);

            StatusMessage =
                paper.IsArchived
                    ? "Paper restored."
                    : "Paper archived.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to update archive state: {exception.Message}";
        }
        finally
        {
            IsUpdatingPaper = false;
        }
    }

    private void RequestDelete()
    {
        IsDeleteConfirmationVisible = true;
    }

    private void CancelDelete()
    {
        IsDeleteConfirmationVisible = false;
    }

    private async Task ConfirmDeleteAsync()
    {
        var paper = SelectedPaper;

        if (paper is null)
        {
            return;
        }

        IsUpdatingPaper = true;

        try
        {
            await _paperLibraryService.DeletePaperAsync(
                paper.Id);

            IsDeleteConfirmationVisible = false;
            SelectedPaper = null;
            Editor.Clear();

            await LoadPapersAsync();

            StatusMessage =
                "Paper deleted permanently.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to delete the paper: {exception.Message}";
        }
        finally
        {
            IsUpdatingPaper = false;
        }
    }

    private async Task SeedDemoDataAsync()
    {
        IsSeeding = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var result =
                await _demoDataSeeder.SeedAsync(150);

            CurrentPage = 1;

            await LoadPapersAsync();

            StatusMessage =
                result.WasCreated
                    ? $"Created {result.PaperCount} papers, " +
                      $"{result.AuthorCount} authors, " +
                      $"{result.TagCount} tags, " +
                      $"{result.NoteCount} notes, and " +
                      $"{result.RelationCount} relationships."
                    : "Demo data already exists.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to create demo data: {exception.Message}";
        }
        finally
        {
            IsSeeding = false;
        }
    }

    private async Task LoadPapersAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        var selectedPaperId =
            SelectedPaper?.Id;

        try
        {
            var result =
                await _paperLibraryService.SearchPapersAsync(
                    new PaperQueryRequest(
                        SearchText,
                        SelectedSortOption,
                        IncludeArchived,
                        FavoritesOnly,
                        CurrentPage,
                        PageSize),
                    cancellationToken);

            Papers.Clear();

            foreach (var paper in result.Items)
            {
                Papers.Add(paper);
            }

            CurrentPage = result.PageNumber;
            TotalCount = result.TotalCount;
            TotalPages = result.TotalPages;

            SelectedPaper =
                selectedPaperId.HasValue
                    ? Papers.FirstOrDefault(
                        paper =>
                            paper.Id ==
                            selectedPaperId.Value)
                    : null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to load the paper library: {exception.Message}";
        }
        finally
        {
            IsLoading = false;
            NotifyCollectionStateChanged();
        }
    }

    private async Task ReloadAndSelectAsync(
        Guid paperId)
    {
        await LoadPapersAsync();

        SelectedPaper = Papers.FirstOrDefault(
            paper => paper.Id == paperId);
    }

    private void HandleSelectedPaperChanged(
        PaperListItem? value)
    {
        _detailsCancellationTokenSource?.Cancel();
        _detailsCancellationTokenSource?.Dispose();
        _detailsCancellationTokenSource = null;

        IsDeleteConfirmationVisible = false;
        SelectedPaperDetails = null;
        Editor.Clear();

        if (value is null)
        {
            IsLoadingDetails = false;
            return;
        }

        _detailsCancellationTokenSource =
            new CancellationTokenSource();

        _ = LoadPaperDetailsAsync(
            value.Id,
            _detailsCancellationTokenSource.Token);
    }

    private async Task LoadPaperDetailsAsync(
        Guid paperId,
        CancellationToken cancellationToken)
    {
        IsLoadingDetails = true;
        ErrorMessage = null;

        try
        {
            var details =
                await _paperLibraryService
                    .GetPaperDetailsAsync(
                        paperId,
                        cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                SelectedPaperDetails = details;
                Editor.Load(details);
            }
        }
        catch (OperationCanceledException)
        {
            // Selection changed before loading completed.
        }
        catch (Exception exception)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                ErrorMessage =
                    $"Unable to load paper details: {exception.Message}";
            }
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IsLoadingDetails = false;
            }
        }
    }

    private async void OnEditorSaved(
        object? sender,
        Guid paperId)
    {
        try
        {
            await ReloadAndSelectAsync(
                paperId);
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "The metadata was saved, but the library " +
                $"could not be refreshed: {exception.Message}";
        }
    }

    private bool CanRunGeneralCommand()
    {
        return !IsBusy;
    }

    private bool CanCreatePaper()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(
                   TitleInput);
    }

    private bool CanGoToPreviousPage()
    {
        return !IsBusy &&
               CanGoPrevious;
    }

    private bool CanGoToNextPage()
    {
        return !IsBusy &&
               CanGoNext;
    }

    private bool CanModifySelectedPaper()
    {
        return !IsBusy &&
               SelectedPaper is not null;
    }

    private bool CanConfirmDelete()
    {
        return CanModifySelectedPaper() &&
               IsDeleteConfirmationVisible;
    }

    private static int? ParsePublicationYear(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!int.TryParse(
                value.Trim(),
                out var publicationYear))
        {
            throw new RequestValidationException(
                "Publication year must be a whole number.");
        }

        return publicationYear;
    }

    private void ClearQuickAddForm()
    {
        TitleInput = string.Empty;
        PublicationYearInput = string.Empty;
        JournalTitleInput = string.Empty;
        DoiInput = string.Empty;
    }

    private void NotifyCollectionStateChanged()
    {
        OnPropertyChanged(nameof(HasPapers));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(PaperCountText));
        OnPropertyChanged(nameof(PageStatusText));
    }

    private void NotifyPagingStateChanged()
    {
        OnPropertyChanged(nameof(PageStatusText));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));

        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
    }

    private void NotifyBusyStateChanged()
    {
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(IsEmpty));

        NotifyCommandStates();
    }

    private void NotifyCommandStates()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        CreatePaperCommand.NotifyCanExecuteChanged();
        SearchCommand.NotifyCanExecuteChanged();
        ClearSearchCommand.NotifyCanExecuteChanged();
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
        ToggleFavoriteCommand.NotifyCanExecuteChanged();
        ToggleArchiveCommand.NotifyCanExecuteChanged();
        RequestDeleteCommand.NotifyCanExecuteChanged();
        ConfirmDeleteCommand.NotifyCanExecuteChanged();
        SeedDemoDataCommand.NotifyCanExecuteChanged();
    }
}