using Avalonia.Controls;
using Avalonia.Input;
using BuildLogDashboard.Models;
using BuildLogDashboard.ViewModels;

namespace BuildLogDashboard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set up StorageProvider and MainWindow reference for dialogs
        this.Loaded += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.StorageProvider = this.StorageProvider;
                vm.MainWindow = this;
            }
        };
    }

    private void OnAppUpdateDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is DataGrid dataGrid
            && dataGrid.SelectedItem is AppUpdate appUpdate
            && DataContext is MainWindowViewModel vm)
        {
            vm.EditAppUpdateCommand.Execute(appUpdate);
        }
    }
}
