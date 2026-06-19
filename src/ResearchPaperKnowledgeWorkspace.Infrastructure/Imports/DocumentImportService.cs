using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Imports;
using ResearchPaperKnowledgeWorkspace.Application.Imports.Models;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Core.Enums;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Imports.Extractors;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Imports;

public sealed class DocumentImportService
    : IDocumentImportService
{
    private readonly IDbContextFactory<
        ResearchWorkspaceDbContext> _dbContextFactory;

    private readonly WorkspacePaths _workspacePaths;

    private readonly IReadOnlyList<
        IDocumentTextExtractor> _extractors;

    public DocumentImportService(
        IDbContextFactory<ResearchWorkspaceDbContext>
            dbContextFactory,
        WorkspacePaths workspacePaths,
        IEnumerable<IDocumentTextExtractor> extractors)
    {
        _dbContextFactory = dbContextFactory;
        _workspacePaths = workspacePaths;
        _extractors = extractors.ToArray();
    }

    public async Task<ImportBatchResult> ImportAsync(
        IReadOnlyCollection<string> sourcePaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourcePaths);

        var normalizedPaths = sourcePaths
            .Where(path =>
                !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var results =
            new List<ImportFileResult>(
                normalizedPaths.Length);

        foreach (var sourcePath in normalizedPaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            results.Add(
                await ImportSingleAsync(
                    sourcePath,
                    cancellationToken));
        }

        return new ImportBatchResult(results);
    }

    public async Task<IReadOnlyList<ImportQueueItem>>
        GetRecentJobsAsync(
            int limit = 100,
            CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 500);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.ImportJobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAtUtc)
            .Take(limit)
            .Select(job =>
                new ImportQueueItem(
                    job.Id,
                    job.OriginalFileName,
                    job.Status,
                    job.AttachmentType,
                    job.DetectedTitle,
                    job.ErrorMessage,
                    job.PaperId,
                    job.CreatedAtUtc,
                    job.CompletedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private async Task<ImportFileResult> ImportSingleAsync(
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var originalFileName =
            Path.GetFileName(sourcePath);

        var importJob = new ImportJob
        {
            OriginalFilePath = sourcePath,
            OriginalFileName = originalFileName,
            Status = ImportJobStatus.Pending
        };

        await using (var creationContext =
                     await _dbContextFactory
                         .CreateDbContextAsync(
                             cancellationToken))
        {
            creationContext.ImportJobs.Add(importJob);

            await creationContext.SaveChangesAsync(
                cancellationToken);
        }

        string? copiedFilePath = null;

        try
        {
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(
                    "The selected file no longer exists.",
                    sourcePath);
            }

            var extension =
                Path.GetExtension(sourcePath);

            var extractor = _extractors.FirstOrDefault(
                item => item.Supports(extension));

            if (extractor is null)
            {
                throw new NotSupportedException(
                    $"Files with the extension '{extension}' are not supported.");
            }

            await using var processingContext =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            var trackedJob =
                await processingContext.ImportJobs
                    .SingleAsync(
                        job => job.Id == importJob.Id,
                        cancellationToken);

            trackedJob.Status =
                ImportJobStatus.Processing;

            trackedJob.AttachmentType =
                extractor.AttachmentType;

            trackedJob.StartedAtUtc =
                DateTimeOffset.UtcNow;

            await processingContext.SaveChangesAsync(
                cancellationToken);

            var sha256Hash =
                await CalculateSha256Async(
                    sourcePath,
                    cancellationToken);

            trackedJob.Sha256Hash = sha256Hash;

            var duplicateExists =
                await processingContext.Attachments
                    .AsNoTracking()
                    .AnyAsync(
                        attachment =>
                            attachment.Sha256Hash ==
                            sha256Hash,
                        cancellationToken);

            if (duplicateExists)
            {
                trackedJob.Status =
                    ImportJobStatus.Duplicate;

                trackedJob.ErrorMessage =
                    "A file with the same SHA-256 hash already exists.";

                trackedJob.CompletedAtUtc =
                    DateTimeOffset.UtcNow;

                await processingContext.SaveChangesAsync(
                    cancellationToken);

                return MapResult(trackedJob);
            }

            var extraction =
                await extractor.ExtractAsync(
                    sourcePath,
                    cancellationToken);

            trackedJob.DetectedTitle =
                Normalize(extraction.Title);

            trackedJob.DetectedAuthorText =
                Normalize(extraction.AuthorText);

            copiedFilePath =
                await CopyToManagedStorageAsync(
                    sourcePath,
                    cancellationToken);

            var relativeFilePath =
                Path.GetRelativePath(
                    _workspacePaths.ApplicationDirectory,
                    copiedFilePath);

            var fileInfo =
                new FileInfo(sourcePath);

            var paper = new Paper
            {
                Title =
                    Normalize(extraction.Title) ??
                    Path.GetFileNameWithoutExtension(
                        sourcePath),

                AbstractText = null,
                ReadingStatus =
                    ReadingStatus.NeedsMetadata
            };

            var attachment = new Attachment
            {
                PaperId = paper.Id,
                Paper = paper,

                OriginalFileName =
                    originalFileName,

                StoredFileName =
                    Path.GetFileName(copiedFilePath),

                FilePath =
                    relativeFilePath,

                MimeType =
                    extractor.MimeType,

                FileExtension =
                    extension.ToLowerInvariant(),

                FileSizeBytes =
                    fileInfo.Length,

                Sha256Hash =
                    sha256Hash,

                ExtractedText =
                    Normalize(extraction.ExtractedText),

                PageCount =
                    extraction.PageCount,

                SourceModifiedAtUtc =
                    new DateTimeOffset(
                        fileInfo.LastWriteTimeUtc),

                AttachmentType =
                    extractor.AttachmentType,

                IsPrimary = true,
                IsFileAvailable = true
            };

            paper.Attachments.Add(attachment);

            processingContext.Papers.Add(paper);

            trackedJob.PaperId = paper.Id;
            trackedJob.AttachmentId = attachment.Id;
            trackedJob.Status = ImportJobStatus.Succeeded;
            trackedJob.ErrorMessage = null;
            trackedJob.CompletedAtUtc =
                DateTimeOffset.UtcNow;

            await processingContext.SaveChangesAsync(
                cancellationToken);

            return MapResult(trackedJob);
        }
        catch (OperationCanceledException)
        {
            await MarkJobFailedAsync(
                importJob.Id,
                ImportJobStatus.Cancelled,
                "The import operation was cancelled.",
                CancellationToken.None);

            DeleteCopiedFile(copiedFilePath);

            throw;
        }
        catch (Exception exception)
        {
            await MarkJobFailedAsync(
                importJob.Id,
                ImportJobStatus.Failed,
                exception.Message,
                CancellationToken.None);

            DeleteCopiedFile(copiedFilePath);

            await using var readContext =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        CancellationToken.None);

            var failedJob =
                await readContext.ImportJobs
                    .AsNoTracking()
                    .SingleAsync(
                        job => job.Id == importJob.Id);

            return MapResult(failedJob);
        }
    }

    private async Task<string> CopyToManagedStorageAsync(
        string sourcePath,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var destinationDirectory = Path.Combine(
            _workspacePaths.FilesDirectory,
            now.Year.ToString("0000"),
            now.Month.ToString("00"));

        Directory.CreateDirectory(destinationDirectory);

        var extension =
            Path.GetExtension(sourcePath)
                .ToLowerInvariant();

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var destinationPath =
            Path.Combine(
                destinationDirectory,
                storedFileName);

        await using var sourceStream =
            new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        await using var destinationStream =
            new FileStream(
                destinationPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous);

        await sourceStream.CopyToAsync(
            destinationStream,
            cancellationToken);

        return destinationPath;
    }

    private static async Task<string>
        CalculateSha256Async(
            string filePath,
            CancellationToken cancellationToken)
    {
        await using var stream =
            new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

        var hash =
            await SHA256.HashDataAsync(
                stream,
                cancellationToken);

        return Convert
            .ToHexString(hash)
            .ToLowerInvariant();
    }

    private async Task MarkJobFailedAsync(
        Guid importJobId,
        ImportJobStatus status,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var job =
            await dbContext.ImportJobs.SingleAsync(
                item => item.Id == importJobId,
                cancellationToken);

        job.Status = status;
        job.ErrorMessage = errorMessage;
        job.CompletedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static ImportFileResult MapResult(
        ImportJob job)
    {
        return new ImportFileResult(
            job.Id,
            job.OriginalFileName,
            job.Status,
            job.PaperId,
            job.AttachmentId,
            job.ErrorMessage);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static void DeleteCopiedFile(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) ||
            !File.Exists(filePath))
        {
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch
        {
            // Cleanup failures must not hide the import error.
        }
    }
}