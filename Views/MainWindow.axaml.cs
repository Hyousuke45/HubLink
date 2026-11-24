using System;
using Avalonia.Controls;
using HubLink.ViewModels;

namespace HubLink.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += OnWindowOpened;
    }

    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm && vm.RefreshCommand.CanExecute(null))
        {
            await vm.RefreshCommand.ExecuteAsync(null);
        }
    }
}