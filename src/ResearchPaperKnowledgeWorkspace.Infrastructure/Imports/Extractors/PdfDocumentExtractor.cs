using System.Text;
using ResearchPaperKnowledgeWorkspace.Core.Enums;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Imports.Extractors;

public sealed class PdfDocumentExtractor
    : IDocumentTextExtractor
{
    public AttachmentType AttachmentType =>
        AttachmentType.Pdf;

    public string MimeType =>
        "application/pdf";

    public bool Supports(string extension)
    {
        return extension.Equals(
            ".pdf",
            StringComparison.OrdinalIgnoreCase);
    }

    public Task<DocumentExtractionResult> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        using var document =
            PdfDocument.Open(filePath);

        var content = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pageText =
                ContentOrderTextExtractor.GetText(page);

            if (string.IsNullOrWhiteSpace(pageText))
            {
                continue;
            }

            content.AppendLine(pageText);
            content.AppendLine();
        }

        return Task.FromResult(
            new DocumentExtractionResult(
                Normalize(document.Information.Title),
                Normalize(document.Information.Author),
                content.ToString().Trim(),
                document.NumberOfPages));
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}