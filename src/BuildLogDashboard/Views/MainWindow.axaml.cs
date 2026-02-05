using Avalonia.Controls;
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
}
