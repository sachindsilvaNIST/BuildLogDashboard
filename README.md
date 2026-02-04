# Android OS Image Build Log Dashboard

A modern, cross-platform GUI application built with **C# and Avalonia UI** for managing Android OS Image build documentation. This dashboard helps AOSP build engineers create, edit, and manage README/changelog files for their Android image builds.

## Features

### Core Functionality
- **Multi-Build Management** - Manage multiple build logs in a single workspace
- **Auto-Scan Detection** - Automatically detects `.zip` and `.json` Android Image files in your workspace
- **SHA256 Checksum Computation** - Calculate file checksums with one click
- **Markdown Import/Export** - Import existing README.md files or export new ones
- **Live Preview** - Real-time markdown preview of your build documentation

### Editor Tabs
| Tab | Description |
|-----|-------------|
| **Build Info** | Build number, date, device, Android version, security patch, kernel version |
| **Changelog** | App updates, system modifications, kernel/driver changes, configuration changes |
| **Issues** | Known issues with severity levels and workarounds |
| **Testing** | Test results with pass/fail/pending status |
| **Release** | Dependencies, release recommendations, customer notes, build engineer info |

### UI Features
- **Light Glass Theme** - Modern frosted panel aesthetic with subtle shadows
- **Responsive Layout** - Sidebar build list with collapsible preview panel
- **Search Functionality** - Filter builds by number, device, or date
- **Status Bar** - Real-time feedback on operations

## Requirements

- **.NET 8.0 SDK** or later
- **Linux** (Ubuntu 20.04+), **Windows 10+**, or **macOS 10.15+**

### Install .NET 8 on Ubuntu
```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

## Quick Start

### Clone and Run
```bash
# Clone the repository
git clone https://github.com/YOUR_USERNAME/Android-OS-Image-Build-Log-Dashboard.git
cd Android-OS-Image-Build-Log-Dashboard/BuildLogDashboard

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project src/BuildLogDashboard
```

### Build from Source
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

## Deployment

### Linux (Self-Contained)
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux
```

### Windows (Self-Contained)
```bash
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/windows
```

### macOS (Self-Contained)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -o ./publish/macos
```

The published output in the `publish/` folder contains a standalone executable that doesn't require .NET to be installed on the target machine.

## User Manual

### Getting Started

#### 1. Open a Workspace
Click **Open Folder** in the toolbar and select the directory containing your Android Image files (`.zip` and `.json` files).

The application will:
- Scan for existing build log files (`BUILD_LOG_*.md`, `README*.md`)
- Detect Android Image files and parse their filenames
- Populate the build list in the sidebar

#### 2. Create a New Build Log
Click **New Build** to create a fresh build log entry. The application will:
- Auto-populate with current date
- Generate a temporary build number
- Add default test entries

#### 3. Edit Build Information

**Build Info Tab:**
- Enter the build number (e.g., `AAL-AA-07009-01`)
- Set the build date using the date picker
- Specify device name, build type, Android version
- Add security patch level and kernel version

**Changelog Tab:**
- Click **+ Add App** to add app updates
- Fill in app name, path, version, and changes
- Use text areas for system modifications, kernel changes, etc.

**Issues Tab:**
- Click **+ Add Issue** to log known issues
- Select severity (Low/Medium/High/Critical)
- Set status (Open/In Progress/Fixed/Won't Fix)
- Document workarounds

**Testing Tab:**
- Click **+ Add Test** to add test entries
- Toggle results between Pass/Fail/Pending/Skipped
- Add notes for each test

**Release Tab:**
- Set bootloader version and compatible OTA builds
- Check release recommendations (Internal Testing / Customer Release)
- Add customer release notes
- Enter build engineer information

#### 4. Preview Your Documentation
Click **Preview** in the toolbar to open the preview panel. Click **Refresh** to update the preview with your latest changes.

#### 5. Save and Export

**Save:** Click **Save** to save the build log as `BUILD_LOG_{BuildNumber}.md` in your workspace.

**Export:** Click **Export** to save the markdown file to a custom location with a custom filename.

#### 6. Import Existing Documentation
Click **Import** to load an existing README.md or BUILD_LOG.md file. The parser will extract:
- Build information from tables
- App updates and changelog entries
- Known issues and test results
- Release information

### File Naming Convention
The application recognizes Android Image files with this pattern:
```
{device}-{buildnum}.{date}.{time}.{ext}
```
Example: `gpn600_001-AAL-AA-07009-01.20260130.062740.zip`

### Keyboard Shortcuts
| Action | Shortcut |
|--------|----------|
| Open Folder | Click toolbar button |
| New Build | Click toolbar button |
| Save | Click toolbar button |
| Toggle Preview | Click toolbar button |

## Project Structure

```
BuildLogDashboard/
├── BuildLogDashboard.sln          # Solution file
├── README.md                       # This file
└── src/
    └── BuildLogDashboard/
        ├── Program.cs              # Application entry point
        ├── App.axaml               # Application configuration
        ├── Converters.cs           # UI value converters
        ├── ViewLocator.cs          # MVVM view locator
        │
        ├── Models/                 # Data models
        │   ├── BuildProject.cs     # Main build data model
        │   ├── BuildFile.cs        # File entry model
        │   ├── AppUpdate.cs        # App changelog model
        │   ├── KnownIssue.cs       # Issue tracking model
        │   └── TestResult.cs       # Test result model
        │
        ├── ViewModels/             # MVVM ViewModels
        │   ├── ViewModelBase.cs    # Base ViewModel class
        │   └── MainWindowViewModel.cs  # Main window logic
        │
        ├── Views/                  # XAML Views
        │   ├── MainWindow.axaml    # Main window UI
        │   └── MainWindow.axaml.cs # Code-behind
        │
        ├── Services/               # Business logic
        │   ├── FileScanner.cs      # Auto-detect image files
        │   ├── MarkdownGenerator.cs # Generate README.md
        │   ├── MarkdownParser.cs   # Parse existing README.md
        │   └── ProjectManager.cs   # Project save/load
        │
        ├── Styles/                 # UI Themes
        │   └── GlassTheme.axaml    # Light glass theme
        │
        └── Assets/                 # Icons and images
```

## Technology Stack

| Component | Technology |
|-----------|------------|
| Framework | .NET 8.0 |
| UI Framework | Avalonia UI 11.3 |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Markdown | Markdig |
| Theme | Fluent + Custom Glass Theme |

## Generated Markdown Format

The application generates README.md files with the following structure:

```markdown
# Android OS Image Build Log - {BuildNumber}

## Build Information
| Property | Value |
|----------|-------|
| Build Number | ... |
| Build Date | ... |
...

## Files
| File | Size | SHA256 |
|------|------|--------|
...

## Changelog
### App Updates
...
### System Modifications
...

## Known Issues
...

## Testing Status
...

## Dependencies
...

## Recommended For
...

## Build Engineer
...
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Avalonia UI](https://avaloniaui.net/) - Cross-platform .NET UI framework
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit
- [Markdig](https://github.com/xoofx/markdig) - Markdown processor

---

**Built for AOSP Build Engineers** - Simplifying Android OS Image documentation management.

DevC: **[Sachin.R.Dsilva](https://github.com/sachindsilvaNIST)**