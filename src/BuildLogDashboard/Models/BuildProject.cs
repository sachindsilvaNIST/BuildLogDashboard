using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLogDashboard.Models;

public partial class BuildProject : ObservableObject
{
    // Static options for dropdowns
    public static string[] AndroidVersionOptions => new[] { "TBD", "14", "13", "12", "11", "10", "9", "8.1", "8.0" };
    public static string[] KernelVersionOptions => new[] { "TBD", "5.15", "5.10", "5.4", "4.19", "4.14", "4.9" };

    // Build Information
    [ObservableProperty]
    private string _buildNumber = string.Empty;

    [ObservableProperty]
    private DateTime _buildDate = DateTime.Now;

    [ObservableProperty]
    private string _device = "TBD";

    [ObservableProperty]
    private string _buildType = "user";

    [ObservableProperty]
    private string _androidVersion = "TBD";

    [ObservableProperty]
    private DateTime? _securityPatchDate = new DateTime(2021, 10, 1);

    // String property for backward compatibility with markdown parsing/generation
    public string SecurityPatch
    {
        get => SecurityPatchDate?.ToString("yyyy-MM-dd") ?? "2021-10-01";
        set
        {
            if (DateTime.TryParse(value, out var date))
            {
                SecurityPatchDate = date;
            }
        }
    }

    [ObservableProperty]
    private string _kernelVersion = "TBD";

    [ObservableProperty]
    private string _previousBuild = "TBD";

    // Files
    [ObservableProperty]
    private ObservableCollection<BuildFile> _files = new();

    // Changelog
    [ObservableProperty]
    private ObservableCollection<AppUpdate> _appUpdates = new();

    [ObservableProperty]
    private string _systemModifications = string.Empty;

    [ObservableProperty]
    private string _kernelDriverChanges = string.Empty;

    [ObservableProperty]
    private string _configurationChanges = string.Empty;

    [ObservableProperty]
    private string _removedComponents = string.Empty;

    // Known Issues
    [ObservableProperty]
    private ObservableCollection<KnownIssue> _knownIssues = new();

    // Testing Status
    [ObservableProperty]
    private ObservableCollection<TestResult> _testResults = new();

    // Dependencies
    [ObservableProperty]
    private string _bootloaderVersion = "TBD";

    [ObservableProperty]
    private string _compatibleOtaBuilds = "TBD";

    // Recommended For
    [ObservableProperty]
    private bool _internalTesting = true;

    [ObservableProperty]
    private bool _customerRelease = false;

    [ObservableProperty]
    private string _specificCustomer = string.Empty;

    // Customer Release Notes
    [ObservableProperty]
    private string _customerReleaseNotes = string.Empty;

    // Build Engineer
    [ObservableProperty]
    private string _builtBy = string.Empty;

    [ObservableProperty]
    private string _reviewedBy = string.Empty;

    [ObservableProperty]
    private DateTime? _approvedForReleaseDate = null;

    // Metadata
    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.Now;

    [ObservableProperty]
    private string _projectFilePath = string.Empty;

    public string DisplayName => string.IsNullOrEmpty(BuildNumber)
        ? "New Build"
        : $"{BuildNumber} - {BuildDate:yyyy-MM-dd}";

    public string ShortName => string.IsNullOrEmpty(BuildNumber)
        ? "New"
        : BuildNumber.Length > 10 ? BuildNumber[..10] + "..." : BuildNumber;

    public BuildProject()
    {
        // Initialize with default test items
        TestResults.Add(new TestResult("Boot Test", "Pending", ""));
        TestResults.Add(new TestResult("Basic Functionality", "Pending", ""));
        TestResults.Add(new TestResult("OTA Update Test", "Pending", ""));
    }
}
