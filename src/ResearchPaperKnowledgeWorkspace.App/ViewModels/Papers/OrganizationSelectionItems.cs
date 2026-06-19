using System;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Models;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.App.ViewModels.Papers;

public sealed class AuthorSelectionItemViewModel
    : ViewModelBase
{
    private bool _isSelected;

    public AuthorSelectionItemViewModel(
        AuthorCatalogItem item)
    {
        Item = item;
    }

    public event EventHandler? SelectionChanged;

    public AuthorCatalogItem Item { get; }

    public Guid Id => Item.Id;

    public string FullName => Item.FullName;

    public string? Orcid => Item.Orcid;

    public string? Affiliation => Item.Affiliation;

    public bool IsSelected
    {
        get => _isSelected;

        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetSelectedSilently(bool value)
    {
        SetProperty(ref _isSelected, value);
    }
}

public sealed class TagSelectionItemViewModel
    : ViewModelBase
{
    private bool _isSelected;

    public TagSelectionItemViewModel(
        TagCatalogItem item)
    {
        Item = item;
    }

    public event EventHandler? SelectionChanged;

    public TagCatalogItem Item { get; }

    public Guid Id => Item.Id;

    public string Name => Item.Name;

    public string? Description => Item.Description;

    public string? ColorHex => Item.ColorHex;

    public bool IsSelected
    {
        get => _isSelected;

        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetSelectedSilently(bool value)
    {
        SetProperty(ref _isSelected, value);
    }
}

public sealed class ProjectSelectionItemViewModel
    : ViewModelBase
{
    private bool _isSelected;

    public ProjectSelectionItemViewModel(
        ResearchProjectCatalogItem item)
    {
        Item = item;
    }

    public event EventHandler? SelectionChanged;

    public ResearchProjectCatalogItem Item { get; }

    public Guid Id => Item.Id;

    public string Name => Item.Name;

    public string? Description => Item.Description;

    public ProjectStatus Status => Item.Status;

    public string? ColorHex => Item.ColorHex;

    public bool IsSelected
    {
        get => _isSelected;

        set
        {
            if (SetProperty(ref _isSelected, value))
            {
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetSelectedSilently(bool value)
    {
        SetProperty(ref _isSelected, value);
    }
}