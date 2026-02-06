using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BuildLogDashboard.Views;

public partial class AppUpdateDialog : Window
{
    private const string PathPrefix = "packages/apps/";
    private bool _isEditMode;

    public bool IsConfirmed { get; private set; }
    public string AppName => AppNameBox.Text?.Trim() ?? string.Empty;
    public string AppPath => PathBox.Text?.Trim() ?? string.Empty;
    public string AppVersion => VersionBox.Text?.Trim() ?? string.Empty;
    public string AppChanges => ChangesBox.Text?.Trim() ?? string.Empty;

    public AppUpdateDialog()
    {
        InitializeComponent();
        AppNameBox.TextChanged += OnAppNameTextChanged;
    }

    /// <summary>
    /// Opens in Edit mode with pre-filled fields.
    /// </summary>
    public AppUpdateDialog(string appName, string path, string version, string changes) : this()
    {
        _isEditMode = true;
        TitleText.Text = "Edit App Update";
        ConfirmButton.Content = "Modify";
        SpinnerText.Text = "Modifying App...";

        AppNameBox.Text = appName;
        PathBox.Text = path;
        VersionBox.Text = version;
        ChangesBox.Text = changes;
    }

    private void OnAppNameTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isEditMode) return;
        var name = AppNameBox.Text?.Trim() ?? string.Empty;
        PathBox.Text = PathPrefix + name;
    }

    private async void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        // Disable buttons and show spinner
        CancelButton.IsEnabled = false;
        ConfirmButton.IsEnabled = false;
        SpinnerOverlay.IsVisible = true;

        await Task.Delay(1000);

        IsConfirmed = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Close();
    }
}
