using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

public sealed class PaperEditorViewModel : ViewModelBase
{
    private readonly PaperLibraryService _paperLibraryService;

    private bool _isApplyingValues;
    private PaperEditSnapshot? _originalSnapshot;

    private Guid _paperId;
    private string _authorsText = string.Empty;
    private DateTimeOffset? _updatedAtUtc;

    private string _title = string.Empty;
    private string _subtitle = string.Empty;
    private string _abstractText = string.Empty;
    private string _publicationYearInput = string.Empty;
    private string _journalTitle = string.Empty;
    private string _conferenceName = string.Empty;
    private string _publisher = string.Empty;
    private string _volume = string.Empty;
    private string _issue = string.Empty;
    private string _pageRange = string.Empty;
    private string _doi = string.Empty;
    private string _isbn = string.Empty;
    private string _issn = string.Empty;
    private string _url = string.Empty;
    private string _languageCode = string.Empty;
    private string _citationKey = string.Empty;

    private ReadingStatus _readingStatus;
    private int _rating;
    private int _priority;
    private bool _isFavorite;

    private bool _isDirty;
    private bool _isSaving;
    private string? _errorMessage;
    private string? _statusMessage;

    public PaperEditorViewModel(
        PaperLibraryService paperLibraryService)
    {
        _paperLibraryService = paperLibraryService;

        SaveCommand = new AsyncRelayCommand(
            SaveAsync,
            CanSave);

        RevertCommand = new RelayCommand(
            Revert,
            CanRevert);
    }

    public event EventHandler<Guid>? Saved;

    public IAsyncRelayCommand SaveCommand { get; }

    public IRelayCommand RevertCommand { get; }

    public IReadOnlyList<ReadingStatus>
        ReadingStatusOptions { get; } =
            Enum.GetValues<ReadingStatus>();

    public IReadOnlyList<int> RatingOptions { get; } =
        Enumerable.Range(0, 6).ToArray();

    public IReadOnlyList<int> PriorityOptions { get; } =
        Enumerable.Range(0, 6).ToArray();

    public Guid PaperId
    {
        get => _paperId;

        private set
        {
            if (SetProperty(ref _paperId, value))
            {
                OnPropertyChanged(nameof(HasPaper));
                OnPropertyChanged(nameof(CanEdit));
                NotifyCommandStates();
            }
        }
    }

    public bool HasPaper =>
        PaperId != Guid.Empty;

    public bool CanEdit =>
        HasPaper && !IsSaving;

    public string AuthorsText
    {
        get => _authorsText;

        private set => SetProperty(
            ref _authorsText,
            value);
    }

    public DateTimeOffset? UpdatedAtUtc
    {
        get => _updatedAtUtc;

        private set
        {
            if (SetProperty(ref _updatedAtUtc, value))
            {
                OnPropertyChanged(nameof(UpdatedAtText));
            }
        }
    }

    public string UpdatedAtText =>
        UpdatedAtUtc?.ToLocalTime()
            .ToString("yyyy-MM-dd HH:mm") ??
        string.Empty;

    public string DirtyStateText =>
        IsDirty
            ? "Unsaved changes"
            : "All changes saved";

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasStatusMessage =>
        !string.IsNullOrWhiteSpace(StatusMessage);

    public string Title
    {
        get => _title;
        set => SetEditorProperty(
            ref _title,
            value ?? string.Empty);
    }

    public string Subtitle
    {
        get => _subtitle;
        set => SetEditorProperty(
            ref _subtitle,
            value ?? string.Empty);
    }

    public string AbstractText
    {
        get => _abstractText;
        set => SetEditorProperty(
            ref _abstractText,
            value ?? string.Empty);
    }

    public string PublicationYearInput
    {
        get => _publicationYearInput;
        set => SetEditorProperty(
            ref _publicationYearInput,
            value ?? string.Empty);
    }

    public string JournalTitle
    {
        get => _journalTitle;
        set => SetEditorProperty(
            ref _journalTitle,
            value ?? string.Empty);
    }

    public string ConferenceName
    {
        get => _conferenceName;
        set => SetEditorProperty(
            ref _conferenceName,
            value ?? string.Empty);
    }

    public string Publisher
    {
        get => _publisher;
        set => SetEditorProperty(
            ref _publisher,
            value ?? string.Empty);
    }

    public string Volume
    {
        get => _volume;
        set => SetEditorProperty(
            ref _volume,
            value ?? string.Empty);
    }

    public string Issue
    {
        get => _issue;
        set => SetEditorProperty(
            ref _issue,
            value ?? string.Empty);
    }

    public string PageRange
    {
        get => _pageRange;
        set => SetEditorProperty(
            ref _pageRange,
            value ?? string.Empty);
    }

    public string Doi
    {
        get => _doi;
        set => SetEditorProperty(
            ref _doi,
            value ?? string.Empty);
    }

    public string Isbn
    {
        get => _isbn;
        set => SetEditorProperty(
            ref _isbn,
            value ?? string.Empty);
    }

    public string Issn
    {
        get => _issn;
        set => SetEditorProperty(
            ref _issn,
            value ?? string.Empty);
    }

    public string Url
    {
        get => _url;
        set => SetEditorProperty(
            ref _url,
            value ?? string.Empty);
    }

    public string LanguageCode
    {
        get => _languageCode;
        set => SetEditorProperty(
            ref _languageCode,
            value ?? string.Empty);
    }

    public string CitationKey
    {
        get => _citationKey;
        set => SetEditorProperty(
            ref _citationKey,
            value ?? string.Empty);
    }

    public ReadingStatus ReadingStatus
    {
        get => _readingStatus;
        set => SetEditorProperty(
            ref _readingStatus,
            value);
    }

    public int Rating
    {
        get => _rating;
        set => SetEditorProperty(
            ref _rating,
            value);
    }

    public int Priority
    {
        get => _priority;
        set => SetEditorProperty(
            ref _priority,
            value);
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetEditorProperty(
            ref _isFavorite,
            value);
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

            OnPropertyChanged(
                nameof(DirtyStateText));

            NotifyCommandStates();
        }
    }

    public bool IsSaving
    {
        get => _isSaving;

        private set
        {
            if (!SetProperty(ref _isSaving, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CanEdit));

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
                OnPropertyChanged(
                    nameof(HasStatusMessage));
            }
        }
    }

    public void Load(PaperDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);

        _isApplyingValues = true;

        try
        {
            PaperId = details.Id;
            AuthorsText = details.AuthorsText;
            UpdatedAtUtc = details.UpdatedAtUtc;

            Title = details.Title;
            Subtitle = details.Subtitle ?? string.Empty;
            AbstractText =
                details.AbstractText ?? string.Empty;

            PublicationYearInput =
                details.PublicationYear?.ToString() ??
                string.Empty;

            JournalTitle =
                details.JournalTitle ?? string.Empty;

            ConferenceName =
                details.ConferenceName ?? string.Empty;

            Publisher =
                details.Publisher ?? string.Empty;

            Volume =
                details.Volume ?? string.Empty;

            Issue =
                details.Issue ?? string.Empty;

            PageRange =
                details.PageRange ?? string.Empty;

            Doi =
                details.Doi ?? string.Empty;

            Isbn =
                details.Isbn ?? string.Empty;

            Issn =
                details.Issn ?? string.Empty;

            Url =
                details.Url ?? string.Empty;

            LanguageCode =
                details.LanguageCode ?? string.Empty;

            CitationKey =
                details.CitationKey ?? string.Empty;

            ReadingStatus =
                details.ReadingStatus;

            Rating =
                details.Rating;

            Priority =
                details.Priority;

            IsFavorite =
                details.IsFavorite;
        }
        finally
        {
            _isApplyingValues = false;
        }

        _originalSnapshot =
            CaptureSnapshot();

        IsDirty = false;
        ErrorMessage = null;
        StatusMessage = null;

        NotifyCommandStates();
    }

    public void Clear()
    {
        _isApplyingValues = true;

        try
        {
            PaperId = Guid.Empty;
            AuthorsText = string.Empty;
            UpdatedAtUtc = null;

            ApplySnapshot(
                PaperEditSnapshot.Empty);
        }
        finally
        {
            _isApplyingValues = false;
        }

        _originalSnapshot = null;

        IsDirty = false;
        ErrorMessage = null;
        StatusMessage = null;

        NotifyCommandStates();
    }

    private async Task SaveAsync()
    {
        IsSaving = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var publicationYear =
                ParsePublicationYear(
                    PublicationYearInput);

            var updatedDetails =
                await _paperLibraryService.UpdatePaperAsync(
                    new UpdatePaperRequest(
                        PaperId,
                        Title,
                        Subtitle,
                        AbstractText,
                        publicationYear,
                        JournalTitle,
                        ConferenceName,
                        Publisher,
                        Volume,
                        Issue,
                        PageRange,
                        Doi,
                        Isbn,
                        Issn,
                        Url,
                        LanguageCode,
                        CitationKey,
                        ReadingStatus,
                        Rating,
                        Priority,
                        IsFavorite));

            Load(updatedDetails);

            StatusMessage =
                "Paper metadata saved successfully.";

            Saved?.Invoke(
                this,
                updatedDetails.Id);
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
                $"Unable to save paper metadata: " +
                exception.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void Revert()
    {
        if (_originalSnapshot is null)
        {
            return;
        }

        _isApplyingValues = true;

        try
        {
            ApplySnapshot(
                _originalSnapshot);
        }
        finally
        {
            _isApplyingValues = false;
        }

        IsDirty = false;
        ErrorMessage = null;

        StatusMessage =
            "Unsaved changes were reverted.";
    }

    private bool CanSave()
    {
        return HasPaper &&
               IsDirty &&
               !IsSaving &&
               !string.IsNullOrWhiteSpace(Title);
    }

    private bool CanRevert()
    {
        return HasPaper &&
               IsDirty &&
               !IsSaving;
    }

    private bool SetEditorProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(
                ref field,
                value,
                propertyName))
        {
            return false;
        }

        MarkDirty();

        return true;
    }

    private void MarkDirty()
    {
        if (_isApplyingValues ||
            !HasPaper ||
            _originalSnapshot is null)
        {
            return;
        }

        IsDirty =
            CaptureSnapshot() !=
            _originalSnapshot;
    }

    private PaperEditSnapshot CaptureSnapshot()
    {
        return new PaperEditSnapshot(
            Title,
            Subtitle,
            AbstractText,
            PublicationYearInput,
            JournalTitle,
            ConferenceName,
            Publisher,
            Volume,
            Issue,
            PageRange,
            Doi,
            Isbn,
            Issn,
            Url,
            LanguageCode,
            CitationKey,
            ReadingStatus,
            Rating,
            Priority,
            IsFavorite);
    }

    private void ApplySnapshot(
        PaperEditSnapshot snapshot)
    {
        Title = snapshot.Title;
        Subtitle = snapshot.Subtitle;
        AbstractText = snapshot.AbstractText;

        PublicationYearInput =
            snapshot.PublicationYearInput;

        JournalTitle =
            snapshot.JournalTitle;

        ConferenceName =
            snapshot.ConferenceName;

        Publisher =
            snapshot.Publisher;

        Volume =
            snapshot.Volume;

        Issue =
            snapshot.Issue;

        PageRange =
            snapshot.PageRange;

        Doi =
            snapshot.Doi;

        Isbn =
            snapshot.Isbn;

        Issn =
            snapshot.Issn;

        Url =
            snapshot.Url;

        LanguageCode =
            snapshot.LanguageCode;

        CitationKey =
            snapshot.CitationKey;

        ReadingStatus =
            snapshot.ReadingStatus;

        Rating =
            snapshot.Rating;

        Priority =
            snapshot.Priority;

        IsFavorite =
            snapshot.IsFavorite;
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

    private void NotifyCommandStates()
    {
        SaveCommand.NotifyCanExecuteChanged();
        RevertCommand.NotifyCanExecuteChanged();
    }

    private sealed record PaperEditSnapshot(
        string Title,
        string Subtitle,
        string AbstractText,
        string PublicationYearInput,
        string JournalTitle,
        string ConferenceName,
        string Publisher,
        string Volume,
        string Issue,
        string PageRange,
        string Doi,
        string Isbn,
        string Issn,
        string Url,
        string LanguageCode,
        string CitationKey,
        ReadingStatus ReadingStatus,
        int Rating,
        int Priority,
        bool IsFavorite)
    {
        public static PaperEditSnapshot Empty { get; } =
            new(
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                ReadingStatus.Unread,
                0,
                0,
                false);
    }
}