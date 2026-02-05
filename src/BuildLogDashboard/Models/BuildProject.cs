using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLogDashboard.Models;

public partial class BuildProject : ObservableObject
{
    // Static options for dropdowns
    public static string[] AndroidVersionOptions => new[] { "TBD", "14", "13", "12", "11", "10", "9", "8.1", "8.0" };
    public static string[] KernelVersionOptions => new[] { "TBD", "5.15", "5.10", "5.4", "4.19", "4.14", "4.9" };
    public static string[] BuildTypeOptions => new[] { "user", "userdebug", "eng" };

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

    // Track if this build was auto-completed from files
    [ObservableProperty]
    private bool _wasAutoCompleted = false;

    // Validation properties for mandatory fields
    public bool IsBuildNumberInvalid => !WasAutoCompleted && string.IsNullOrWhiteSpace(BuildNumber);
    public bool IsDeviceInvalid => !WasAutoCompleted && (string.IsNullOrWhiteSpace(Device) || Device == "TBD");
    public bool IsAndroidVersionInvalid => string.IsNullOrWhiteSpace(AndroidVersion) || AndroidVersion == "TBD";
    public bool IsBuildTypeInvalid => string.IsNullOrWhiteSpace(BuildType);

    // Testing validation - check if required tests are still Pending
    public bool IsBootTestInvalid => TestResults.FirstOrDefault(t => t.TestName == "Boot Test")?.Result == "Pending";
    public bool IsBasicFunctionalityInvalid => TestResults.FirstOrDefault(t => t.TestName == "Basic Functionality")?.Result == "Pending";
    public bool IsOtaTestInvalid => TestResults.FirstOrDefault(t => t.TestName == "OTA Update Test")?.Result == "Pending";

    // Recommended For validation - at least one must be checked
    public bool IsRecommendedForInvalid => !InternalTesting && !CustomerRelease;

    // Build Engineer validation
    public bool IsBuiltByInvalid => string.IsNullOrWhiteSpace(BuiltBy);
    public bool IsReviewedByInvalid => string.IsNullOrWhiteSpace(ReviewedBy);
    public bool IsApprovedDateInvalid => !ApprovedForReleaseDate.HasValue;

    // Overall validation check
    public bool HasValidationErrors => IsBuildNumberInvalid || IsDeviceInvalid || IsAndroidVersionInvalid ||
                                       IsBuildTypeInvalid || IsBootTestInvalid || IsBasicFunctionalityInvalid ||
                                       IsOtaTestInvalid || IsRecommendedForInvalid || IsBuiltByInvalid ||
                                       IsReviewedByInvalid || IsApprovedDateInvalid;

    // Refresh validation when properties change
    partial void OnBuildNumberChanged(string value) => NotifyValidationChanged();
    partial void OnDeviceChanged(string value) => NotifyValidationChanged();
    partial void OnAndroidVersionChanged(string value) => NotifyValidationChanged();
    partial void OnBuildTypeChanged(string value) => NotifyValidationChanged();
    partial void OnInternalTestingChanged(bool value) => NotifyValidationChanged();
    partial void OnCustomerReleaseChanged(bool value) => NotifyValidationChanged();
    partial void OnBuiltByChanged(string value) => NotifyValidationChanged();
    partial void OnReviewedByChanged(string value) => NotifyValidationChanged();
    partial void OnApprovedForReleaseDateChanged(DateTime? value) => NotifyValidationChanged();

    private void NotifyValidationChanged()
    {
        OnPropertyChanged(nameof(IsBuildNumberInvalid));
        OnPropertyChanged(nameof(IsDeviceInvalid));
        OnPropertyChanged(nameof(IsAndroidVersionInvalid));
        OnPropertyChanged(nameof(IsBuildTypeInvalid));
        OnPropertyChanged(nameof(IsRecommendedForInvalid));
        OnPropertyChanged(nameof(IsBuiltByInvalid));
        OnPropertyChanged(nameof(IsReviewedByInvalid));
        OnPropertyChanged(nameof(IsApprovedDateInvalid));
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    public void RefreshTestValidation()
    {
        OnPropertyChanged(nameof(IsBootTestInvalid));
        OnPropertyChanged(nameof(IsBasicFunctionalityInvalid));
        OnPropertyChanged(nameof(IsOtaTestInvalid));
        OnPropertyChanged(nameof(HasValidationErrors));
    }

    public BuildProject()
    {
        // Initialize with default test items
        TestResults.Add(new TestResult("Boot Test", "Pending", ""));
        TestResults.Add(new TestResult("Basic Functionality", "Pending", ""));
        TestResults.Add(new TestResult("OTA Update Test", "Pending", ""));
    }
}
