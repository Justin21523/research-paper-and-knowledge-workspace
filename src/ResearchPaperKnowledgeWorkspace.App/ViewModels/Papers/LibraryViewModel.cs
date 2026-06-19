using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ResearchPaperKnowledgeWorkspace.App.ViewModels;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

public sealed class LibraryViewModel : ViewModelBase
{
    private readonly PaperLibraryService _paperLibraryService;

    private bool _isInitialized;
    private CancellationTokenSource? _detailsCancellationTokenSource;

    private string _titleInput = string.Empty;
    private string _publicationYearInput = string.Empty;
    private string _journalTitleInput = string.Empty;
    private string _doiInput = string.Empty;

    private string? _errorMessage;
    private string? _statusMessage;

    private bool _isLoading;
    private bool _isCreating;
    private bool _isLoadingDetails;

    private PaperListItem? _selectedPaper;
    private PaperDetails? _selectedPaperDetails;

    public LibraryViewModel(
        PaperLibraryService paperLibraryService)
    {
        _paperLibraryService = paperLibraryService;

        RefreshCommand = new AsyncRelayCommand(
            RefreshAsync,
            CanRefresh);

        CreatePaperCommand = new AsyncRelayCommand(
            CreatePaperAsync,
            CanCreatePaper);
    }

    public ObservableCollection<PaperListItem> Papers { get; } =
        new();

    public IAsyncRelayCommand RefreshCommand { get; }

    public IAsyncRelayCommand CreatePaperCommand { get; }

    public bool HasPapers => Papers.Count > 0;

    public bool IsEmpty =>
        !IsLoading &&
        Papers.Count == 0;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasStatusMessage =>
        !string.IsNullOrWhiteSpace(StatusMessage);

    public bool IsBusy =>
        IsLoading ||
        IsCreating;

    public int PaperCount => Papers.Count;

    public string PaperCountText =>
        PaperCount == 1
            ? "1 paper"
            : $"{PaperCount} papers";

    public bool HasSelectedPaper =>
        SelectedPaper is not null;

    public bool HasNoSelectedPaper =>
        SelectedPaper is null;

    public bool HasSelectedPaperDetails =>
        SelectedPaperDetails is not null &&
        !IsLoadingDetails;

    public string TitleInput
    {
        get => _titleInput;

        set
        {
            if (SetProperty(
                    ref _titleInput,
                    value ?? string.Empty))
            {
                CreatePaperCommand.NotifyCanExecuteChanged();
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
            if (!SetProperty(ref _isLoading, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsEmpty));

            RefreshCommand.NotifyCanExecuteChanged();
            CreatePaperCommand.NotifyCanExecuteChanged();
        }
    }

    public bool IsCreating
    {
        get => _isCreating;

        private set
        {
            if (!SetProperty(ref _isCreating, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsBusy));

            RefreshCommand.NotifyCanExecuteChanged();
            CreatePaperCommand.NotifyCanExecuteChanged();
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

            HandleSelectedPaperChanged(value);
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

    private async Task RefreshAsync()
    {
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

            await _paperLibraryService.CreatePaperAsync(
                new CreatePaperRequest(
                    Title: TitleInput,
                    PublicationYear: publicationYear,
                    JournalTitle: JournalTitleInput,
                    Doi: DoiInput));

            ClearQuickAddForm();

            await LoadPapersAsync();

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

    private async Task LoadPapersAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = null;

        var selectedPaperId = SelectedPaper?.Id;

        try
        {
            var papers =
                await _paperLibraryService.GetPaperListAsync(
                    cancellationToken);

            Papers.Clear();

            foreach (var paper in papers)
            {
                Papers.Add(paper);
            }

            SelectedPaper = selectedPaperId.HasValue
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
                $"Unable to load the paper library: " +
                exception.Message;
        }
        finally
        {
            IsLoading = false;

            NotifyCollectionStateChanged();
        }
    }

    private void HandleSelectedPaperChanged(
        PaperListItem? value)
    {
        _detailsCancellationTokenSource?.Cancel();
        _detailsCancellationTokenSource?.Dispose();
        _detailsCancellationTokenSource = null;

        SelectedPaperDetails = null;

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
            }
        }
        catch (OperationCanceledException)
        {
            // The selected paper changed before loading completed.
        }
        catch (Exception exception)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                ErrorMessage =
                    $"Unable to load paper details: " +
                    exception.Message;
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

    private bool CanCreatePaper()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(TitleInput);
    }

    private bool CanRefresh()
    {
        return !IsBusy;
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
        OnPropertyChanged(nameof(PaperCount));
        OnPropertyChanged(nameof(PaperCountText));
    }
}