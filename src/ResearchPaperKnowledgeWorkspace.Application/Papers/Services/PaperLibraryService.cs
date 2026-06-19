using System;
using System.Threading;
using System.Threading.Tasks;

using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Application.Common.Models;

namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Services;

public sealed class PaperLibraryService
{
    private readonly IPaperRepository _paperRepository;

    public PaperLibraryService(IPaperRepository paperRepository)
    {
        _paperRepository = paperRepository;
    }

    public async Task<IReadOnlyList<PaperListItem>>
        GetPaperListAsync(
            CancellationToken cancellationToken = default)
    {
        var papers = await _paperRepository.ListAsync(
            cancellationToken);

        return papers
            .Select(MapListItem)
            .ToList();
    }

    public async Task<Guid> CreatePaperAsync(
        CreatePaperRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var title = NormalizeRequiredText(
            request.Title,
            "Paper title is required.");

        ValidatePublicationYear(request.PublicationYear);

        var paper = new Paper
        {
            Title = title,
            PublicationYear = request.PublicationYear,
            JournalTitle = NormalizeOptionalText(
                request.JournalTitle),
            Doi = NormalizeOptionalText(request.Doi),
            AbstractText = NormalizeOptionalText(
                request.AbstractText)
        };

        await _paperRepository.AddAsync(
            paper,
            cancellationToken);

        return paper.Id;
    }

    private static string BuildAuthorsText(Paper paper)
    {
        var authorNames = paper.PaperAuthors
            .OrderBy(paperAuthor => paperAuthor.AuthorOrder)
            .Select(paperAuthor => paperAuthor.Author.FullName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        return authorNames.Count == 0
            ? string.Empty
            : string.Join(", ", authorNames);
    }

    private static string NormalizeRequiredText(
        string? value,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RequestValidationException(
                errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static void ValidatePublicationYear(
        int? publicationYear)
    {
        if (publicationYear is null)
        {
            return;
        }

        var maximumYear = DateTime.UtcNow.Year + 5;

        if (publicationYear < 1000 ||
            publicationYear > maximumYear)
        {
            throw new RequestValidationException(
                $"Publication year must be between 1000 and {maximumYear}.");
        }
    }
    public async Task<PaperDetails> GetPaperDetailsAsync(
        Guid paperId,
        CancellationToken cancellationToken = default)
    {
        if (paperId == Guid.Empty)
        {
            throw new RequestValidationException(
                "A valid paper identifier is required.");
        }

        var paper = await _paperRepository.GetByIdAsync(
            paperId,
            cancellationToken);

        if (paper is null)
        {
            throw new EntityNotFoundException(
                "The selected paper could not be found.");
        }

        return new PaperDetails(
            paper.Id,
            paper.Title,
            paper.Subtitle,
            BuildAuthorsText(paper),
            paper.AbstractText,
            paper.PublicationYear,
            paper.JournalTitle,
            paper.ConferenceName,
            paper.Publisher,
            paper.Volume,
            paper.Issue,
            paper.PageRange,
            paper.Doi,
            paper.Isbn,
            paper.Issn,
            paper.Url,
            paper.LanguageCode,
            paper.CitationKey,
            paper.ReadingStatus,
            paper.Rating,
            paper.Priority,
            paper.IsFavorite,
            paper.IsArchived,
            paper.UpdatedAtUtc);
    }
    public async Task<PaperDetails> UpdatePaperAsync(
        UpdatePaperRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Id == Guid.Empty)
        {
            throw new RequestValidationException(
                "A valid paper identifier is required.");
        }

        ValidatePublicationYear(
            request.PublicationYear);

        ValidateZeroToFive(
            request.Rating,
            "Rating");

        ValidateZeroToFive(
            request.Priority,
            "Priority");

        var paper = await _paperRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (paper is null)
        {
            throw new EntityNotFoundException(
                "The selected paper could not be found.");
        }

        paper.Title = NormalizeRequiredText(
            request.Title,
            "Paper title is required.");

        paper.Subtitle =
            NormalizeOptionalText(request.Subtitle);

        paper.AbstractText =
            NormalizeOptionalText(request.AbstractText);

        paper.PublicationYear =
            request.PublicationYear;

        paper.JournalTitle =
            NormalizeOptionalText(request.JournalTitle);

        paper.ConferenceName =
            NormalizeOptionalText(request.ConferenceName);

        paper.Publisher =
            NormalizeOptionalText(request.Publisher);

        paper.Volume =
            NormalizeOptionalText(request.Volume);

        paper.Issue =
            NormalizeOptionalText(request.Issue);

        paper.PageRange =
            NormalizeOptionalText(request.PageRange);

        paper.Doi =
            NormalizeOptionalText(request.Doi);

        paper.Isbn =
            NormalizeOptionalText(request.Isbn);

        paper.Issn =
            NormalizeOptionalText(request.Issn);

        paper.Url =
            NormalizeOptionalText(request.Url);

        paper.LanguageCode =
            NormalizeOptionalText(request.LanguageCode);

        paper.CitationKey =
            NormalizeOptionalText(request.CitationKey);

        paper.ReadingStatus =
            request.ReadingStatus;

        paper.Rating =
            request.Rating;

        paper.Priority =
            request.Priority;

        paper.IsFavorite =
            request.IsFavorite;

        await _paperRepository.UpdateAsync(
            paper,
            cancellationToken);

        return await GetPaperDetailsAsync(
            paper.Id,
            cancellationToken);
    }
    private static void ValidateZeroToFive(
        int value,
        string fieldName)
    {
        if (value < 0 || value > 5)
        {
            throw new RequestValidationException(
                $"{fieldName} must be between 0 and 5.");
        }
    }    

    public async Task<PagedResult<PaperListItem>>
        SearchPapersAsync(
            PaperQueryRequest request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.PageNumber < 1)
        {
            throw new RequestValidationException(
                "Page number must be at least 1.");
        }

        if (request.PageSize is < 1 or > 100)
        {
            throw new RequestValidationException(
                "Page size must be between 1 and 100.");
        }

        var result = await _paperRepository.QueryAsync(
            request,
            cancellationToken);

        var items = result.Items
            .Select(MapListItem)
            .ToList();

        return new PagedResult<PaperListItem>(
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize);
    }

    public async Task SetFavoriteAsync(
        Guid paperId,
        bool isFavorite,
        CancellationToken cancellationToken = default)
    {
        if (!await _paperRepository.SetFavoriteAsync(
                paperId,
                isFavorite,
                cancellationToken))
        {
            throw new EntityNotFoundException(
                "The selected paper could not be found.");
        }
    }

    public async Task SetArchivedAsync(
        Guid paperId,
        bool isArchived,
        CancellationToken cancellationToken = default)
    {
        if (!await _paperRepository.SetArchivedAsync(
                paperId,
                isArchived,
                cancellationToken))
        {
            throw new EntityNotFoundException(
                "The selected paper could not be found.");
        }
    }

    public async Task DeletePaperAsync(
        Guid paperId,
        CancellationToken cancellationToken = default)
    {
        if (!await _paperRepository.DeleteAsync(
                paperId,
                cancellationToken))
        {
            throw new EntityNotFoundException(
                "The selected paper could not be found.");
        }
    }
    private static PaperListItem MapListItem(
        Paper paper)
    {
        return new PaperListItem(
            paper.Id,
            paper.Title,
            BuildAuthorsText(paper),
            paper.PublicationYear,
            paper.JournalTitle,
            paper.ReadingStatus,
            paper.IsFavorite,
            paper.IsArchived,
            paper.UpdatedAtUtc);
    }

}