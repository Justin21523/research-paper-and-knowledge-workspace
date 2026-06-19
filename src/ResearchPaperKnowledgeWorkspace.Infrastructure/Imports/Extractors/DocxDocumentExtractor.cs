using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Imports.Extractors;

public sealed class DocxDocumentExtractor
    : IDocumentTextExtractor
{
    public AttachmentType AttachmentType =>
        AttachmentType.WordDocument;

    public string MimeType =>
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public bool Supports(string extension)
    {
        return extension.Equals(
            ".docx",
            StringComparison.OrdinalIgnoreCase);
    }

    public Task<DocumentExtractionResult> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var document =
            WordprocessingDocument.Open(
                filePath,
                false);

        var body =
            document.MainDocumentPart?
                .Document
                .Body;

        var paragraphs = body?
            .Descendants<Paragraph>()
            .Select(paragraph =>
                string.Concat(
                    paragraph
                        .Descendants<Text>()
                        .Select(text => text.Text)))
            .Where(text =>
                !string.IsNullOrWhiteSpace(text))
            .ToArray() ??
            [];

        var extractedText =
            string.Join(
                Environment.NewLine,
                paragraphs);

        var title =
            Normalize(document.PackageProperties.Title) ??
            paragraphs.FirstOrDefault();

        var author =
            Normalize(document.PackageProperties.Creator);

        return Task.FromResult(
            new DocumentExtractionResult(
                title,
                author,
                extractedText,
                null));
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}