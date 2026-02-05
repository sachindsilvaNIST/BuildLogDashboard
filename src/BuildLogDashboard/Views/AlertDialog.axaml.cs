using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BuildLogDashboard.Views;

public partial class AlertDialog : Window
{
    public AlertDialog()
    {
        InitializeComponent();
    }

    public AlertDialog(string title, string message) : this()
    {
        TitleText.Text = title;
        MessageText.Text = message;
    }

    public static async System.Threading.Tasks.Task ShowAsync(Window owner, string title, string message)
    {
        var dialog = new AlertDialog(title, message);
        await dialog.ShowDialog(owner);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
