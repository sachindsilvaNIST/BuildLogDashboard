using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLogDashboard.Models;

public partial class KnownIssue : ObservableObject
{
    [ObservableProperty]
    private string _issue = string.Empty;

    [ObservableProperty]
    private string _severity = "Medium";

    [ObservableProperty]
    private string _status = "Open";

    [ObservableProperty]
    private string _workaround = string.Empty;

    public static string[] SeverityOptions => new[] { "Low", "Medium", "High", "Critical" };
    public static string[] StatusOptions => new[] { "Open", "In Progress", "Fixed", "Won't Fix" };

    public KnownIssue() { }

    public KnownIssue(string issue, string severity, string status, string workaround)
    {
        Issue = issue;
        Severity = severity;
        Status = status;
        Workaround = workaround;
    }
}
