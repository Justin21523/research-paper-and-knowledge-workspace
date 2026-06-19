using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ResearchPaperKnowledgeWorkspace.App.ViewModels.Imports;

namespace ResearchPaperKnowledgeWorkspace.App.Views.Imports;

public partial class ImportWorkspaceView : UserControl
{
    private static readonly FilePickerFileType
        SupportedDocuments =
            new("Research documents")
            {
                Patterns =
                [
                    "*.pdf",
                    "*.docx",
                    "*.md",
                    "*.markdown"
                ]
            };

    public ImportWorkspaceView()
    {
        InitializeComponent();
    }

    private async void OnOpenFilesClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not
            ImportWorkspaceViewModel viewModel)
        {
            return;
        }

        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel?.StorageProvider.CanOpen != true)
        {
            viewModel.ReportError(
                "The current platform cannot open a file picker.");

            return;
        }

        try
        {
            var files =
                await topLevel.StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title =
                                "Import research documents",

                            AllowMultiple = true,

                            FileTypeFilter =
                            [
                                SupportedDocuments
                            ]
                        });

            var paths = files
                .Select(file =>
                    file.TryGetLocalPath())
                .Where(path =>
                    !string.IsNullOrWhiteSpace(path))
                .Cast<string>()
                .ToArray();

            await viewModel.ImportPathsAsync(paths);
        }
        catch (Exception exception)
        {
            viewModel.ReportError(
                $"Unable to open files: {exception.Message}");
        }
    }

    private void OnDragEnter(
        object? sender,
        DragEventArgs e)
    {
        var acceptsFiles =
            e.DataTransfer.Formats.Any(
                format => format == DataFormat.File);

        SetDragState(acceptsFiles);
    }

    private void OnDragLeave(
        object? sender,
        DragEventArgs e)
    {
        SetDragState(false);
    }

    private void OnDragOver(
        object? sender,
        DragEventArgs e)
    {
        var acceptsFiles =
            e.DataTransfer.Formats.Contains(
                DataFormat.File);

        e.DragEffects =
            acceptsFiles
                ? DragDropEffects.Copy
                : DragDropEffects.None;

        SetDragState(acceptsFiles);
    }

    private async void OnDrop(
        object? sender,
        DragEventArgs e)
    {
        SetDragState(false);

        if (DataContext is not
            ImportWorkspaceViewModel viewModel)
        {
            return;
        }

        var files =
            e.DataTransfer.TryGetFiles();

        if (files is null)
        {
            return;
        }

        var paths = files
            .Select(file =>
                file.TryGetLocalPath())
            .Where(path =>
                !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToArray();

        await viewModel.ImportPathsAsync(paths);
    }

    private async void OnOpenAttachmentClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not
                ImportWorkspaceViewModel viewModel ||
            viewModel.SelectedAttachment is not
                { IsFileAvailable: true } attachment)
        {
            return;
        }

        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            return;
        }

        try
        {
            await topLevel.Launcher
                .LaunchFileInfoAsync(
                    new FileInfo(
                        attachment.AbsoluteFilePath));
        }
        catch (Exception exception)
        {
            viewModel.ReportError(
                $"Unable to open the file: {exception.Message}");
        }
    }

    private async void OnRevealAttachmentClick(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not
                ImportWorkspaceViewModel viewModel ||
            viewModel.SelectedAttachment is not
                { IsFileAvailable: true } attachment)
        {
            return;
        }

        var directoryPath =
            Path.GetDirectoryName(
                attachment.AbsoluteFilePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            return;
        }

        try
        {
            await topLevel.Launcher
                .LaunchDirectoryInfoAsync(
                    new DirectoryInfo(directoryPath));
        }
        catch (Exception exception)
        {
            viewModel.ReportError(
                $"Unable to open the folder: {exception.Message}");
        }
    }

    private void SetDragState(bool isActive)
    {
        if (DataContext is
            ImportWorkspaceViewModel viewModel)
        {
            viewModel.SetDragActive(isActive);
        }
    }
}