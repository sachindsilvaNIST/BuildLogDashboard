using Avalonia.Controls;
using BuildLogDashboard.ViewModels;

namespace BuildLogDashboard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Set up StorageProvider for file dialogs
        this.Loaded += (s, e) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.StorageProvider = this.StorageProvider;
            }
        };
    }
}
