using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BuildLogDashboard.Models;
using BuildLogDashboard.Services;

namespace BuildLogDashboard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ProjectManager _projectManager;
    private readonly HtmlGenerator _htmlGenerator;
    private readonly PdfGenerator _pdfGenerator;

    // Default starting directory for file dialogs
    private const string DefaultStartDirectory = "/home/sankyo/Sachin Files/01 NIST - AEM979/NIST - Resources (ALL) [NIST - RESOURCE &  PERMISSION/GPN600-001 RESOURCES/[GPN600-001] Android Images";

    // Default export directory for saving files
    private const string DefaultExportDirectory = "/home/sankyo/Sachin Files/01 NIST - AEM979/NIST - Resources (ALL) [NIST - RESOURCE &  PERMISSION/GPN600-001 RESOURCES/[GPN600-001] Android Image Build Documentations";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWorkspaceLoaded))]
    private string _workspacePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BuildProject> _builds = new();

    [ObservableProperty]
    private BuildProject? _selectedBuild;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isPreviewVisible = false;

    [ObservableProperty]
    private string _previewContent = string.Empty;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _isBusy = false;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _busyMessage = "Loading...";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWorkspaceLoaded))]
    private bool _hasBuildsLoaded = false;

    [ObservableProperty]
    private bool _isSaved = false;

    // Computed property for button enable state
    public bool HasWorkspaceLoaded => HasBuildsLoaded || !string.IsNullOrEmpty(WorkspacePath);

    // Save button enabled only if no validation errors
    public bool CanSave => HasWorkspaceLoaded && SelectedBuild != null && !SelectedBuild.HasValidationErrors;

    // Export button enabled only if no validation errors
    public bool CanExport => HasWorkspaceLoaded && SelectedBuild != null && !SelectedBuild.HasValidationErrors;

    // For file dialog
    public IStorageProvider? StorageProvider { get; set; }

    // For showing dialogs
    public Avalonia.Controls.Window? MainWindow { get; set; }

    public MainWindowViewModel()
    {
        _projectManager = new ProjectManager();
        _htmlGenerator = new HtmlGenerator(_projectManager.MarkdownGenerator);
        _pdfGenerator = new PdfGenerator();
    }

    partial void OnSelectedBuildChanged(BuildProject? oldValue, BuildProject? newValue)
    {
        // Unsubscribe from old build's changes
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= OnBuildPropertyChanged;
            UnsubscribeFromCollections(oldValue);
        }

        // Subscribe to new build's changes
        if (newValue != null)
        {
            newValue.PropertyChanged += OnBuildPropertyChanged;
            SubscribeToCollections(newValue);

            if (IsPreviewVisible)
            {
                UpdatePreview();
            }
        }

        // Reset saved state when switching builds
        IsSaved = false;

        // Update button enable states
        OnPropertyChanged(nameof(CanSave));
        OnPropertyChanged(nameof(CanExport));
    }

    private void SubscribeToCollections(BuildProject build)
    {
        build.Files.CollectionChanged += OnCollectionChanged;
        build.AppUpdates.CollectionChanged += OnCollectionChanged;
        build.KnownIssues.CollectionChanged += OnCollectionChanged;
        build.TestResults.CollectionChanged += OnCollectionChanged;

        // Subscribe to each item's PropertyChanged
        foreach (var item in build.Files) item.PropertyChanged += OnBuildPropertyChanged;
        foreach (var item in build.AppUpdates) item.PropertyChanged += OnBuildPropertyChanged;
        foreach (var item in build.KnownIssues) item.PropertyChanged += OnBuildPropertyChanged;
        foreach (var item in build.TestResults) item.PropertyChanged += OnBuildPropertyChanged;
    }

    private void UnsubscribeFromCollections(BuildProject build)
    {
        build.Files.CollectionChanged -= OnCollectionChanged;
        build.AppUpdates.CollectionChanged -= OnCollectionChanged;
        build.KnownIssues.CollectionChanged -= OnCollectionChanged;
        build.TestResults.CollectionChanged -= OnCollectionChanged;

        foreach (var item in build.Files) item.PropertyChanged -= OnBuildPropertyChanged;
        foreach (var item in build.AppUpdates) item.PropertyChanged -= OnBuildPropertyChanged;
        foreach (var item in build.KnownIssues) item.PropertyChanged -= OnBuildPropertyChanged;
        foreach (var item in build.TestResults) item.PropertyChanged -= OnBuildPropertyChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to new items
        if (e.NewItems != null)
        {
            foreach (INotifyPropertyChanged item in e.NewItems)
            {
                item.PropertyChanged += OnBuildPropertyChanged;
            }
        }

        // Unsubscribe from removed items
        if (e.OldItems != null)
        {
            foreach (INotifyPropertyChanged item in e.OldItems)
            {
                item.PropertyChanged -= OnBuildPropertyChanged;
            }
        }

        // Refresh test validation if TestResults collection changed
        if (sender == SelectedBuild?.TestResults)
        {
            SelectedBuild?.RefreshTestValidation();
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(CanExport));
        }

        // Mark as unsaved when collection changes
        IsSaved = false;
    }

    private void OnBuildPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Refresh test validation when test result changes
        if (sender is TestResult && e.PropertyName == nameof(TestResult.Result))
        {
            SelectedBuild?.RefreshTestValidation();
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(CanExport));
        }

        // Update button states when validation-related properties change
        if (e.PropertyName == nameof(BuildProject.HasValidationErrors) ||
            e.PropertyName?.StartsWith("Is") == true)  // IsXxxInvalid properties
        {
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(CanExport));
        }

        // Don't mark as unsaved for LastUpdated changes (that's set during save)
        if (e.PropertyName != nameof(BuildProject.LastUpdated))
        {
            IsSaved = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterBuilds();
    }

    [RelayCommand]
    private async Task OpenFolderAsync()
    {
        if (StorageProvider == null) return;

        var startFolder = await StorageProvider.TryGetFolderFromPathAsync(DefaultStartDirectory);

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Android Image Workspace",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder
        });

        if (folders.Count > 0)
        {
            WorkspacePath = folders[0].Path.LocalPath;
            _projectManager.SetWorkspace(WorkspacePath);
            await LoadBuildsAsync();
        }
    }

    [RelayCommand]
    private async Task LoadBuildsAsync()
    {
        if (string.IsNullOrEmpty(WorkspacePath)) return;

        IsBusy = true;
        BusyMessage = "Loading builds...";
        StatusMessage = "Loading builds...";

        try
        {
            await Task.Run(() =>
            {
                var projects = _projectManager.LoadAllProjects();
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    Builds.Clear();
                    foreach (var project in projects)
                    {
                        Builds.Add(project);
                    }

                    if (Builds.Count > 0)
                    {
                        SelectedBuild = Builds[0];
                        HasBuildsLoaded = true;
                        StatusMessage = $"Loaded {Builds.Count} build(s)";
                    }
                    else
                    {
                        // No valid builds found - reset to welcome state
                        HasBuildsLoaded = false;
                        WorkspacePath = string.Empty;
                        SelectedBuild = null;
                        StatusMessage = "No valid build files found (.zip/.json). Please select a valid workspace.";
                    }
                });
            });
        }
        catch (Exception ex)
        {
            // Reset state on error
            HasBuildsLoaded = false;
            WorkspacePath = string.Empty;
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewBuild()
    {
        var newProject = _projectManager.CreateNewProject();
        newProject.BuildDate = DateTime.Now;
        newProject.BuildNumber = $"NEW-{DateTime.Now:yyyyMMdd-HHmmss}";

        Builds.Insert(0, newProject);
        SelectedBuild = newProject;
        StatusMessage = "Created new build";
    }

    [RelayCommand]
    private async Task SaveBuildAsync()
    {
        if (SelectedBuild == null) return;

        IsBusy = true;
        BusyMessage = "Saving...";
        StatusMessage = "Saving...";

        try
        {
            SelectedBuild.LastUpdated = DateTime.Now;
            await _projectManager.SaveProjectAsync(SelectedBuild);

            // Add 2-second delay for visual feedback
            await Task.Delay(2000);

            IsSaved = true;
            StatusMessage = "Saved successfully";
        }
        catch (Exception ex)
        {
            IsSaved = false;
            StatusMessage = $"Save failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportMarkdownAsync()
    {
        if (SelectedBuild == null || MainWindow == null) return;

        var suggestedName = string.IsNullOrEmpty(SelectedBuild.BuildNumber)
            ? "BUILD_LOG.md"
            : $"BUILD_LOG_{SelectedBuild.BuildNumber}.md";

        var defaultFilePath = System.IO.Path.Combine(DefaultExportDirectory, suggestedName);

        // Generate preview content
        var previewContent = _projectManager.GeneratePreview(SelectedBuild);

        // Show preview dialog with integrated save location
        var previewWindow = new Views.ExportPreviewWindow();
        previewWindow.SetPreviewContent(previewContent, "Markdown (.md)", defaultFilePath, ".md");
        await previewWindow.ShowDialog(MainWindow);

        if (!previewWindow.IsConfirmed)
        {
            StatusMessage = "Export cancelled";
            return;
        }

        var filePath = previewWindow.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            StatusMessage = "Export cancelled - no file path specified";
            return;
        }

        IsBusy = true;
        BusyMessage = "Exporting...";
        StatusMessage = "Exporting...";

        try
        {
            await _projectManager.ExportAsMarkdownAsync(SelectedBuild, filePath);

            // Add 2-second delay for visual feedback
            await Task.Delay(2000);

            StatusMessage = $"Exported to {System.IO.Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportHtmlAsync()
    {
        if (SelectedBuild == null || MainWindow == null) return;

        var suggestedName = string.IsNullOrEmpty(SelectedBuild.BuildNumber)
            ? "BUILD_LOG.html"
            : $"BUILD_LOG_{SelectedBuild.BuildNumber}.html";

        var defaultFilePath = System.IO.Path.Combine(DefaultExportDirectory, suggestedName);

        // Generate preview content
        var previewContent = _projectManager.GeneratePreview(SelectedBuild);

        // Show preview dialog with integrated save location
        var previewWindow = new Views.ExportPreviewWindow();
        previewWindow.SetPreviewContent(previewContent, "HTML (.html)", defaultFilePath, ".html");
        await previewWindow.ShowDialog(MainWindow);

        if (!previewWindow.IsConfirmed)
        {
            StatusMessage = "Export cancelled";
            return;
        }

        var filePath = previewWindow.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            StatusMessage = "Export cancelled - no file path specified";
            return;
        }

        IsBusy = true;
        BusyMessage = "Exporting...";
        StatusMessage = "Exporting...";

        try
        {
            SelectedBuild.LastUpdated = DateTime.Now;
            var html = _htmlGenerator.Generate(SelectedBuild);
            await System.IO.File.WriteAllTextAsync(filePath, html);

            // Add 2-second delay for visual feedback
            await Task.Delay(2000);

            StatusMessage = $"Exported to {System.IO.Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (SelectedBuild == null || MainWindow == null) return;

        var suggestedName = string.IsNullOrEmpty(SelectedBuild.BuildNumber)
            ? "BUILD_LOG.pdf"
            : $"BUILD_LOG_{SelectedBuild.BuildNumber}.pdf";

        var defaultFilePath = System.IO.Path.Combine(DefaultExportDirectory, suggestedName);

        // Generate PDF preview images
        IsBusy = true;
        BusyMessage = "Generating preview...";
        StatusMessage = "Generating preview...";

        System.Collections.Generic.List<byte[]>? previewImages = null;
        try
        {
            previewImages = await Task.Run(() => _pdfGenerator.GeneratePreviewImages(SelectedBuild));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Preview generation failed: {ex.Message}";
            IsBusy = false;
            return;
        }
        finally
        {
            IsBusy = false;
        }

        // Show preview dialog with PDF images
        var previewWindow = new Views.ExportPreviewWindow();
        previewWindow.SetPreviewImages(previewImages, "PDF (.pdf)", defaultFilePath, ".pdf");
        await previewWindow.ShowDialog(MainWindow);

        if (!previewWindow.IsConfirmed)
        {
            StatusMessage = "Export cancelled";
            return;
        }

        var filePath = previewWindow.FilePath;
        if (string.IsNullOrEmpty(filePath))
        {
            StatusMessage = "Export cancelled - no file path specified";
            return;
        }

        IsBusy = true;
        BusyMessage = "Generating PDF...";
        StatusMessage = "Generating PDF...";

        try
        {
            SelectedBuild.LastUpdated = DateTime.Now;

            // Run PDF generation on background thread with explicit error capture
            var pdfError = await Task.Run(() =>
            {
                try
                {
                    _pdfGenerator.Generate(SelectedBuild, filePath);
                    return (string?)null;
                }
                catch (Exception pdfEx)
                {
                    // Capture full error including inner exception
                    var errorMsg = pdfEx.Message;
                    if (pdfEx.InnerException != null)
                    {
                        errorMsg += $" | Inner: {pdfEx.InnerException.Message}";
                    }
                    // Also log to console for debugging
                    Console.WriteLine($"PDF Error: {pdfEx}");
                    return errorMsg;
                }
            });

            if (pdfError != null)
            {
                StatusMessage = $"PDF failed: {pdfError}";
                return;
            }

            // Add 2-second delay for visual feedback
            await Task.Delay(2000);

            StatusMessage = $"Exported to {System.IO.Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportMarkdownAsync()
    {
        if (StorageProvider == null) return;

        var startFolder = await StorageProvider.TryGetFolderFromPathAsync(DefaultStartDirectory);

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Markdown File",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Markdown") { Patterns = new[] { "*.md" } }
            }
        });

        if (files.Count > 0)
        {
            IsBusy = true;
            BusyMessage = "Importing...";
            StatusMessage = "Importing...";

            try
            {
                var project = await _projectManager.ImportMarkdownAsync(files[0].Path.LocalPath);
                Builds.Insert(0, project);
                SelectedBuild = project;
                HasBuildsLoaded = true;
                StatusMessage = "Imported successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Import failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void TogglePreview()
    {
        IsPreviewVisible = !IsPreviewVisible;
        if (IsPreviewVisible && SelectedBuild != null)
        {
            UpdatePreview();
        }
    }

    [RelayCommand]
    private void UpdatePreview()
    {
        if (SelectedBuild == null)
        {
            PreviewContent = "";
            return;
        }

        PreviewContent = _projectManager.GeneratePreview(SelectedBuild);
    }

    [RelayCommand]
    private async Task ComputeChecksumsAsync()
    {
        if (SelectedBuild == null) return;

        IsBusy = true;
        BusyMessage = "Computing checksums...";
        StatusMessage = "Computing checksums...";

        try
        {
            await _projectManager.ComputeFileChecksumsAsync(SelectedBuild);
            StatusMessage = "Checksums computed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMarkdownFileAsync()
    {
        if (StorageProvider == null) return;

        var startFolder = await StorageProvider.TryGetFolderFromPathAsync(DefaultStartDirectory);

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Markdown File to Delete",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Markdown") { Patterns = new[] { "*.md" } }
            }
        });

        if (files.Count > 0)
        {
            try
            {
                System.IO.File.Delete(files[0].Path.LocalPath);
                StatusMessage = $"Deleted {files[0].Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Delete failed: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task DeleteHtmlFileAsync()
    {
        if (StorageProvider == null) return;

        var startFolder = await StorageProvider.TryGetFolderFromPathAsync(DefaultStartDirectory);

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select HTML File to Delete",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("HTML") { Patterns = new[] { "*.html" } }
            }
        });

        if (files.Count > 0)
        {
            try
            {
                System.IO.File.Delete(files[0].Path.LocalPath);
                StatusMessage = $"Deleted {files[0].Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Delete failed: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private async Task DeletePdfFileAsync()
    {
        if (StorageProvider == null) return;

        var startFolder = await StorageProvider.TryGetFolderFromPathAsync(DefaultStartDirectory);

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select PDF File to Delete",
            AllowMultiple = false,
            SuggestedStartLocation = startFolder,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PDF") { Patterns = new[] { "*.pdf" } }
            }
        });

        if (files.Count > 0)
        {
            try
            {
                System.IO.File.Delete(files[0].Path.LocalPath);
                StatusMessage = $"Deleted {files[0].Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Delete failed: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    private void DeleteBuild()
    {
        if (SelectedBuild == null) return;

        var index = Builds.IndexOf(SelectedBuild);
        Builds.Remove(SelectedBuild);

        if (Builds.Count > 0)
        {
            SelectedBuild = Builds[Math.Min(index, Builds.Count - 1)];
        }
        else
        {
            SelectedBuild = null;
        }

        StatusMessage = "Build removed from list";
    }

    // App Updates
    [RelayCommand]
    private void AddAppUpdate()
    {
        SelectedBuild?.AppUpdates.Add(new AppUpdate
        {
            AppName = "New App",
            Path = "packages/apps/",
            Version = "1.0.0",
            Changes = "Description"
        });
    }

    [RelayCommand]
    private void RemoveAppUpdate(AppUpdate? appUpdate)
    {
        if (appUpdate != null && SelectedBuild != null)
        {
            SelectedBuild.AppUpdates.Remove(appUpdate);
        }
    }

    // Known Issues
    [RelayCommand]
    private void AddKnownIssue()
    {
        SelectedBuild?.KnownIssues.Add(new KnownIssue
        {
            Issue = "New Issue",
            Severity = "Medium",
            Status = "Open",
            Workaround = "-"
        });
    }

    [RelayCommand]
    private void RemoveKnownIssue(KnownIssue? issue)
    {
        if (issue != null && SelectedBuild != null)
        {
            SelectedBuild.KnownIssues.Remove(issue);
        }
    }

    // Test Results
    [RelayCommand]
    private void AddTestResult()
    {
        SelectedBuild?.TestResults.Add(new TestResult
        {
            TestName = "New Test",
            Result = "Pending",
            Notes = ""
        });
    }

    [RelayCommand]
    private void RemoveTestResult(TestResult? testResult)
    {
        if (testResult != null && SelectedBuild != null)
        {
            SelectedBuild.TestResults.Remove(testResult);
        }
    }

    // Files
    [RelayCommand]
    private void AddFile()
    {
        SelectedBuild?.Files.Add(new BuildFile
        {
            FileName = "new_file.zip",
            FileSize = "0 B",
            Sha256 = "-"
        });
    }

    [RelayCommand]
    private void RemoveFile(BuildFile? file)
    {
        if (file != null && SelectedBuild != null)
        {
            SelectedBuild.Files.Remove(file);
        }
    }

    private void FilterBuilds()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return;
        }

        var searchLower = SearchText.ToLower();
        var filtered = Builds.Where(b =>
            b.BuildNumber.ToLower().Contains(searchLower) ||
            b.Device.ToLower().Contains(searchLower) ||
            b.BuildDate.ToString("yyyy-MM-dd").Contains(searchLower)
        ).ToList();
    }
}
