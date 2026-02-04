using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLogDashboard.Models;

public partial class TestResult : ObservableObject
{
    [ObservableProperty]
    private string _testName = string.Empty;

    [ObservableProperty]
    private string _result = "Pending";

    [ObservableProperty]
    private string _notes = string.Empty;

    public static string[] ResultOptions => new[] { "Pass", "Fail", "Pending", "Skipped" };

    public TestResult() { }

    public TestResult(string testName, string result, string notes)
    {
        TestName = testName;
        Result = result;
        Notes = notes;
    }
}
