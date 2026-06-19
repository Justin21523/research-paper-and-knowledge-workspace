using System;
using System.Threading;
using System.Threading.Tasks;

using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Persistence;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Application.Papers.Services;

public sealed class PaperLibraryService
{
    private readonly IPaperRepository _paperRepository;

    public PaperLibraryService(IPaperRepository paperRepository)
    {
        _paperRepository = paperRepository;
    }

    public async Task<IReadOnlyList<PaperListItem>> GetPaperListAsync(
        CancellationToken cancellationToken = default)
    {
        var papers = await _paperRepository.ListAsync(
            cancellationToken);

        return papers
            .Select(
                paper => new PaperListItem(
                    paper.Id,
                    paper.Title,
                    BuildAuthorsText(paper),
                    paper.PublicationYear,
                    paper.JournalTitle,
                    paper.ReadingStatus,
                    paper.IsFavorite,
                    paper.UpdatedAtUtc))
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
            paper.UpdatedAtUtc);
    }

}