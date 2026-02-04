using CommunityToolkit.Mvvm.ComponentModel;

namespace BuildLogDashboard.Models;

public partial class BuildFile : ObservableObject
{
    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _fileSize = string.Empty;

    [ObservableProperty]
    private string _sha256 = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    public BuildFile() { }

    public BuildFile(string fileName, string fileSize, string sha256 = "-")
    {
        FileName = fileName;
        FileSize = fileSize;
        Sha256 = sha256;
    }
}
