using System;
using Avalonia.Controls;
using ResearchPaperKnowledgeWorkspace.App.ViewModels;

namespace ResearchPaperKnowledgeWorkspace.App.Views;

public partial class MainWindow : Window
{
    private bool _isInitialized;

    public MainWindow()
    {
        InitializeComponent();
        Opened += OnOpened;
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;

        if (DataContext is MainWindowViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}