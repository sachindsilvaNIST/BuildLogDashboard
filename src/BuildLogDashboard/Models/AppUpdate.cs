using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLogDashboard.Models;

public partial class AppUpdate : ObservableObject
{
    [ObservableProperty]
    private string _appName = string.Empty;

    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _changes = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _details = new();

    public AppUpdate() { }

    public AppUpdate(string appName, string path, string version, string changes)
    {
        AppName = appName;
        Path = path;
        Version = version;
        Changes = changes;
    }
}
