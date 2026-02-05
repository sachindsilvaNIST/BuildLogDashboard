using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
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

    /// <summary>
    /// Sets the preview content using PDF page images
    /// </summary>
    public void SetPreviewImages(List<byte[]> pageImages, string exportType, string defaultFilePath, string fileExtension)
    {
        ExportTypeText.Text = $"Exporting as {exportType}";
        FilePathTextBox.Text = defaultFilePath;
        _exportType = exportType;
        _fileExtension = fileExtension;
        _storageProvider = this.StorageProvider;

        // Convert byte arrays to Bitmaps and display
        var bitmaps = new List<Bitmap>();
        foreach (var imageBytes in pageImages)
        {
            using var stream = new MemoryStream(imageBytes);
            var bitmap = new Bitmap(stream);
            bitmaps.Add(bitmap);
        }

        PagesContainer.ItemsSource = bitmaps;
        PageCountText.Text = $"{bitmaps.Count} page{(bitmaps.Count != 1 ? "s" : "")}";
    }

    /// <summary>
    /// Sets the preview content using plain text (fallback for non-PDF exports)
    /// </summary>
    public void SetPreviewContent(string markdownContent, string exportType, string defaultFilePath, string fileExtension)
    {
        ExportTypeText.Text = $"Exporting as {exportType}";
        FilePathTextBox.Text = defaultFilePath;
        _exportType = exportType;
        _fileExtension = fileExtension;
        _storageProvider = this.StorageProvider;

        // For text preview, create a simple text display
        var textBlock = new TextBlock
        {
            Text = markdownContent,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontFamily = new Avalonia.Media.FontFamily("Consolas, Monaco, monospace"),
            FontSize = 12,
            Margin = new Avalonia.Thickness(20)
        };

        var border = new Border
        {
            Background = Avalonia.Media.Brushes.White,
            CornerRadius = new Avalonia.CornerRadius(4),
            Margin = new Avalonia.Thickness(20, 8),
            Padding = new Avalonia.Thickness(20),
            Child = textBlock
        };

        PagesContainer.ItemsSource = new List<Border> { border };
        PageCountText.Text = "Text Preview";
    }

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        if (_storageProvider == null) return;

        try
        {
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
        var filePath = FilePathTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(filePath))
        {
            StatusText.Text = "Please enter a valid file path";
            StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D13438"));
            return;
        }

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
