using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Imports.Extractors;

public sealed record DocumentExtractionResult(
    string? Title,
    string? AuthorText,
    string ExtractedText,
    int? PageCount);

public interface IDocumentTextExtractor
{
    AttachmentType AttachmentType { get; }

    string MimeType { get; }

    bool Supports(string extension);

    Task<DocumentExtractionResult> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}