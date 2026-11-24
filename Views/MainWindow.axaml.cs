using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using HubLink.ViewModels;

namespace HubLink.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Opened += OnWindowOpened;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SelectSaveFilePathAsync = PromptSaveCsvPathAsync;
        }
    }

    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.RefreshCommand.CanExecute(null))
        {
            await vm.RefreshCommand.ExecuteAsync(null);
        }
    }

    private async Task<string?> PromptSaveCsvPathAsync(string suggestedFileName)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is not { } storageProvider)
        {
            return null;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export Displayed Shipments to CSV",
            SuggestedFileName = suggestedFileName,
            DefaultExtension = "csv",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("CSV (Comma delimited)")
                {
                    Patterns = new[] { "*.csv" },
                    MimeTypes = new[] { "text/csv" }
                }
            }
        });

        return file?.TryGetLocalPath();
    }
}