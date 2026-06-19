using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Imports.Extractors;

public sealed class MarkdownDocumentExtractor
    : IDocumentTextExtractor
{
    public AttachmentType AttachmentType =>
        AttachmentType.Markdown;

    public string MimeType =>
        "text/markdown";

    public bool Supports(string extension)
    {
        return extension.Equals(
                   ".md",
                   StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(
                   ".markdown",
                   StringComparison.OrdinalIgnoreCase);
    }

    public async Task<DocumentExtractionResult> ExtractAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(
            filePath,
            cancellationToken);

        var title = content
            .Split('\n')
            .Select(line => line.Trim())
            .FirstOrDefault(line =>
                line.StartsWith("# ", StringComparison.Ordinal))
            ?[2..]
            .Trim();

        return new DocumentExtractionResult(
            title,
            null,
            content,
            null);
    }
}