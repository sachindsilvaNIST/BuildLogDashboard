using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace BuildLogDashboard.Views;

public partial class ExportPreviewWindow : Window
{
    public bool IsConfirmed { get; private set; } = false;
    public string FilePath => FilePathTextBox?.Text ?? string.Empty;

    private string _exportType = "File";
    private string _fileExtension = ".md";
    private IStorageProvider? _storageProvider;

    public ExportPreviewWindow()
    {
        InitializeComponent();
    }

    public void SetPreviewContent(string markdownContent, string exportType, string defaultFilePath, string fileExtension)
    {
        PreviewMarkdown.Markdown = markdownContent;
        ExportTypeText.Text = $"Exporting as {exportType}";
        FilePathTextBox.Text = defaultFilePath;
        _exportType = exportType;
        _fileExtension = fileExtension;
        _storageProvider = this.StorageProvider;
    }

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        if (_storageProvider == null) return;

        try
        {
            // Try to get the directory from current path
            var currentPath = FilePathTextBox.Text ?? string.Empty;
            var directory = System.IO.Path.GetDirectoryName(currentPath);
            var fileName = System.IO.Path.GetFileName(currentPath);

            IStorageFolder? startFolder = null;
            if (!string.IsNullOrEmpty(directory) && System.IO.Directory.Exists(directory))
            {
                startFolder = await _storageProvider.TryGetFolderFromPathAsync(directory);
            }

            var fileTypes = _fileExtension switch
            {
                ".pdf" => new FilePickerFileType[] { new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } } },
                ".html" => new FilePickerFileType[] { new FilePickerFileType("HTML") { Patterns = new[] { "*.html" } } },
                _ => new FilePickerFileType[] { new FilePickerFileType("Markdown") { Patterns = new[] { "*.md" } } }
            };

            var file = await _storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Save as {_exportType}",
                SuggestedFileName = fileName,
                SuggestedStartLocation = startFolder,
                FileTypeChoices = fileTypes
            });

            if (file != null)
            {
                FilePathTextBox.Text = file.Path.LocalPath;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Browse error: {ex.Message}";
        }
    }

    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        // Validate file path
        var filePath = FilePathTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(filePath))
        {
            StatusText.Text = "Please enter a valid file path";
            StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D13438"));
            return;
        }

        // Ensure directory exists
        var directory = System.IO.Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            try
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Cannot create directory: {ex.Message}";
                StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D13438"));
                return;
            }
        }

        IsConfirmed = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        IsConfirmed = false;
        Close();
    }
}
