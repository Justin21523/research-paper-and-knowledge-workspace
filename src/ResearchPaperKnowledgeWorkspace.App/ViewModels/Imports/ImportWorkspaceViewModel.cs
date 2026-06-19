using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using ResearchPaperKnowledgeWorkspace.App.ViewModels;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Imports;
using ResearchPaperKnowledgeWorkspace.Application.Imports.Models;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Imports;

public sealed class ImportWorkspaceViewModel : ViewModelBase
{
    private static readonly HashSet<string>
        SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".docx",
            ".md",
            ".markdown"
        };

    private readonly IDocumentImportService
        _documentImportService;

    private bool _isInitialized;
    private bool _isBusy;
    private bool _isDragActive;

    private ImportQueueItem? _selectedJob;
    private AttachmentListItem? _selectedAttachment;

    private string? _errorMessage;
    private string? _statusMessage;

    public ImportWorkspaceViewModel(
        IDocumentImportService documentImportService)
    {
        _documentImportService = documentImportService;

        RefreshQueueCommand = new AsyncRelayCommand(
            () => RefreshQueueWithBusyStateAsync(
                CancellationToken.None),
            CanRunCommand);

        ClearCompletedCommand = new AsyncRelayCommand(
            ClearCompletedAsync,
            CanRunCommand);
    }

    public event EventHandler? ImportCompleted;

    public ObservableCollection<ImportQueueItem>
        Jobs { get; } = [];

    public ObservableCollection<AttachmentListItem>
        Attachments { get; } = [];

    public IAsyncRelayCommand RefreshQueueCommand { get; }

    public IAsyncRelayCommand RetrySelectedCommand { get; }

    public IAsyncRelayCommand ClearCompletedCommand { get; }

    public bool HasJobs =>
        Jobs.Count > 0;

    public bool IsQueueEmpty =>
        !IsBusy &&
        Jobs.Count == 0;

    public bool HasAttachments =>
        Attachments.Count > 0;

    public bool HasNoAttachments =>
        SelectedJob?.PaperId is not null &&
        Attachments.Count == 0;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasStatusMessage =>
        !string.IsNullOrWhiteSpace(StatusMessage);

    public bool CanOpenSelectedAttachment =>
        SelectedAttachment is
        {
            IsFileAvailable: true
        };

    public string QueueSummaryText =>
        $"{Jobs.Count} recent jobs · " +
        $"{Jobs.Count(job => job.Status == ImportJobStatus.Succeeded)} succeeded · " +
        $"{Jobs.Count(job => job.Status == ImportJobStatus.Duplicate)} duplicates · " +
        $"{Jobs.Count(job => job.Status == ImportJobStatus.Failed)} failed";

    public string DropZoneText =>
        IsDragActive
            ? "Release to import documents"
            : "Drop PDF, DOCX, Markdown, or .md files here";

    public bool IsBusy
    {
        get => _isBusy;

        private set
        {
            if (!SetProperty(ref _isBusy, value))
            {
                return;
            }

            OnPropertyChanged(nameof(IsQueueEmpty));
            NotifyCommandStates();
        }
    }

    public bool IsDragActive
    {
        get => _isDragActive;

        private set
        {
            if (SetProperty(ref _isDragActive, value))
            {
                OnPropertyChanged(nameof(DropZoneText));
            }
        }
    }

    public ImportQueueItem? SelectedJob
    {
        get => _selectedJob;

        set
        {
            if (!SetProperty(ref _selectedJob, value))
            {
                return;
            }

            SelectedAttachment = null;

            RetrySelectedCommand.NotifyCanExecuteChanged();

            _ = LoadSelectedJobAttachmentsAsync();
        }
    }

    public AttachmentListItem? SelectedAttachment
    {
        get => _selectedAttachment;

        set
        {
            if (SetProperty(
                    ref _selectedAttachment,
                    value))
            {
                OnPropertyChanged(
                    nameof(CanOpenSelectedAttachment));
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

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        await RefreshQueueWithBusyStateAsync(
                cancellationToken);
    }

    public async Task ImportPathsAsync(
        IEnumerable<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourcePaths);

        var validPaths = sourcePaths
            .Where(path =>
                !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Where(path =>
                SupportedExtensions.Contains(
                    Path.GetExtension(path)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (validPaths.Length == 0)
        {
            ReportError(
                "No supported PDF, DOCX, or Markdown files were selected.");

            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var result =
                await _documentImportService.ImportAsync(
                    validPaths,
                    cancellationToken);

            await RefreshQueueCoreAsync(
                cancellationToken);

            StatusMessage =
                $"Processed {result.TotalCount} files: " +
                $"{result.SucceededCount} imported, " +
                $"{result.DuplicateCount} duplicates, " +
                $"{result.FailedCount} failed.";

            if (result.SucceededCount > 0)
            {
                ImportCompleted?.Invoke(
                    this,
                    EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to import documents: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SetDragActive(bool value)
    {
        IsDragActive = value;
    }

    public void ReportError(string message)
    {
        ErrorMessage = message;
        StatusMessage = null;
    }

    private async Task RefreshQueueWithBusyStateAsync(
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            await RefreshQueueCoreAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to load import history: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshQueueCoreAsync(
        CancellationToken cancellationToken)
    {
        var selectedJobId =
            SelectedJob?.Id;

        var jobs =
            await _documentImportService
                .GetRecentJobsAsync(
                    200,
                    cancellationToken);

        Jobs.Clear();

        foreach (var job in jobs)
        {
            Jobs.Add(job);
        }

        SelectedJob =
            selectedJobId.HasValue
                ? Jobs.FirstOrDefault(job =>
                    job.Id == selectedJobId.Value)
                : Jobs.FirstOrDefault();

        NotifyQueueStateChanged();
    }

    private async Task RetrySelectedAsync()
    {
        var job = SelectedJob;

        if (job is null)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            var result =
                await _documentImportService.RetryAsync(
                    job.Id);

            await RefreshQueueCoreAsync(
                CancellationToken.None);

            StatusMessage =
                result.Status == ImportJobStatus.Succeeded
                    ? "The document was imported successfully."
                    : $"Retry completed with status: {result.Status}.";

            if (result.Status ==
                ImportJobStatus.Succeeded)
            {
                ImportCompleted?.Invoke(
                    this,
                    EventArgs.Empty);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to retry import: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearCompletedAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var removedCount =
                await _documentImportService
                    .ClearCompletedJobsAsync();

            SelectedJob = null;
            Attachments.Clear();

            await RefreshQueueCoreAsync(
                CancellationToken.None);

            StatusMessage =
                $"Removed {removedCount} completed import records.";
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to clear import history: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSelectedJobAttachmentsAsync()
    {
        Attachments.Clear();
        SelectedAttachment = null;

        var paperId = SelectedJob?.PaperId;

        if (paperId is null ||
            paperId == Guid.Empty)
        {
            NotifyAttachmentStateChanged();
            return;
        }

        try
        {
            var attachments =
                await _documentImportService
                    .GetPaperAttachmentsAsync(
                        paperId.Value);

            foreach (var attachment in attachments)
            {
                Attachments.Add(attachment);
            }

            SelectedAttachment =
                Attachments.FirstOrDefault();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Unable to load attachments: {exception.Message}";
        }
        finally
        {
            NotifyAttachmentStateChanged();
        }
    }

    private bool CanRunCommand()
    {
        return !IsBusy;
    }

    private bool CanRetrySelected()
    {
        return !IsBusy &&
               SelectedJob?.Status is
                   ImportJobStatus.Failed or
                   ImportJobStatus.Cancelled;
    }

    private void NotifyQueueStateChanged()
    {
        OnPropertyChanged(nameof(HasJobs));
        OnPropertyChanged(nameof(IsQueueEmpty));
        OnPropertyChanged(nameof(QueueSummaryText));

        NotifyCommandStates();
    }

    private void NotifyAttachmentStateChanged()
    {
        OnPropertyChanged(nameof(HasAttachments));
        OnPropertyChanged(nameof(HasNoAttachments));
        OnPropertyChanged(
            nameof(CanOpenSelectedAttachment));
    }

    private void NotifyCommandStates()
    {
        RefreshQueueCommand.NotifyCanExecuteChanged();
        RetrySelectedCommand.NotifyCanExecuteChanged();
        ClearCompletedCommand.NotifyCanExecuteChanged();
    }
}