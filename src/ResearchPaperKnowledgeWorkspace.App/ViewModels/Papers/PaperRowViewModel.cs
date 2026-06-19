using System;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

public sealed class PaperRowViewModel : ViewModelBase
{
    private bool _isBatchSelected;

    public PaperRowViewModel(
        PaperListItem item,
        bool isBatchSelected = false)
    {
        Item = item ??
            throw new ArgumentNullException(nameof(item));

        _isBatchSelected = isBatchSelected;
    }

    public event Action<Guid, bool>? BatchSelectionChanged;

    public PaperListItem Item { get; }

    public Guid Id => Item.Id;

    public string Title => Item.Title;

    public string AuthorsText => Item.AuthorsText;

    public int? PublicationYear => Item.PublicationYear;

    public string? JournalTitle => Item.JournalTitle;

    public ReadingStatus ReadingStatus => Item.ReadingStatus;

    public bool IsFavorite => Item.IsFavorite;

    public bool IsArchived => Item.IsArchived;

    public DateTimeOffset UpdatedAtUtc => Item.UpdatedAtUtc;

    public string FavoriteMarker =>
        IsFavorite ? "★" : string.Empty;

    public string ArchiveMarker =>
        IsArchived ? "Archived" : string.Empty;

    public bool IsBatchSelected
    {
        get => _isBatchSelected;

        set
        {
            if (!SetProperty(
                    ref _isBatchSelected,
                    value))
            {
                return;
            }

            BatchSelectionChanged?.Invoke(
                Id,
                value);
        }
    }
}