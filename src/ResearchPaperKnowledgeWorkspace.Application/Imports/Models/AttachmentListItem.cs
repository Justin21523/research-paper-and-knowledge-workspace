using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Application.Imports.Models;

public sealed record AttachmentListItem(
    Guid Id,
    Guid PaperId,
    string OriginalFileName,
    AttachmentType AttachmentType,
    long FileSizeBytes,
    int? PageCount,
    string AbsoluteFilePath,
    bool IsFileAvailable)
{
    public string FileSizeText =>
        FileSizeBytes switch
        {
            >= 1_073_741_824 =>
                $"{FileSizeBytes / 1_073_741_824d:F2} GB",

            >= 1_048_576 =>
                $"{FileSizeBytes / 1_048_576d:F2} MB",

            >= 1024 =>
                $"{FileSizeBytes / 1024d:F1} KB",

            _ =>
                $"{FileSizeBytes} bytes"
        };

    public string PageCountText =>
        PageCount is null
            ? string.Empty
            : $"{PageCount} pages";
}