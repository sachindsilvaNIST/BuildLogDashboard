# Android OS Image Build Log Dashboard

A modern, cross-platform GUI application built with **C# and Avalonia UI** for managing Android OS Image build documentation. This dashboard helps AOSP build engineers create, edit, and manage README/changelog files for their Android image builds.

## Features

### Core Functionality
- **Multi-Build Management** - Manage multiple build logs in a single workspace
- **Auto-Scan Detection** - Automatically detects `.zip` and `.json` Android Image files in your workspace
- **SHA256 Checksum Computation** - Calculate file checksums with one click
- **Markdown Import/Export** - Import existing README.md files or export new ones
- **Multi-Format Export** - Export build logs as Markdown (.md), HTML (.html), or PDF (.pdf)
- **Export Preview** - Preview your document before exporting with an integrated export dialog
- **PDF Generation** - Professional PDF output with styled tables, color-coded status badges, and page headers/footers
- **Rendered Markdown Preview** - Live rendered markdown preview panel (tables, headers, lists styled properly)
- **Mandatory Field Validation** - Apple-style popup alerts when required fields are missing before Save/Export
- **Auto-Complete from Filenames** - Build number, device, and date auto-populated from Android image filenames

### Editor Tabs
| Tab | Description |
|-----|-------------|
| **Build Info** | Build number, date, device, build type, Android version, security patch, kernel version |
| **Changelog** | App updates, system modifications, kernel/driver changes, configuration changes |
| **Issues** | Known issues with severity levels and workarounds |
| **Testing** | Test results with pass/fail/pending status |
| **Release** | Dependencies, release recommendations, customer notes, build engineer info |

### UI Features
- **Light Glass Theme** - Modern frosted panel aesthetic with subtle shadows
- **Responsive Layout** - Sidebar build list with collapsible preview panel
- **Search Functionality** - Filter builds by number, device, or date
- **Apple-Style Alert Dialogs** - Validation popups for missing mandatory fields
- **Loading Spinner** - Apple-style circular spinner overlay during operations
- **Status Bar** - Real-time feedback on operations

### Validation
The following fields are validated before Save/Export:
- Build Number, Device, Build Type, Android Version
- Boot Test, Basic Functionality, OTA Update Test (must not be "Pending")
- Recommended For (at least one option selected)
- Built by, Reviewed by, Approved Date

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
dotnet build src/BuildLogDashboard/BuildLogDashboard.csproj

# Release build
dotnet build src/BuildLogDashboard/BuildLogDashboard.csproj -c Release
```

## Deployment

### Linux (Self-Contained)
```bash
dotnet publish src/BuildLogDashboard/BuildLogDashboard.csproj -c Release -r linux-x64 --self-contained -o ./publish
```

### Windows (Self-Contained)
```bash
dotnet publish src/BuildLogDashboard/BuildLogDashboard.csproj -c Release -r win-x64 --self-contained -o ./publish/windows
```

### macOS (Self-Contained)
```bash
dotnet publish src/BuildLogDashboard/BuildLogDashboard.csproj -c Release -r osx-x64 --self-contained -o ./publish/macos
```

The published output in the `publish/` folder contains a standalone executable that doesn't require .NET to be installed on the target machine.

### Ubuntu Desktop Launcher
To add the application to your Ubuntu application launcher, create a `.desktop` file:

```bash
nano ~/.local/share/applications/buildlogdashboard.desktop
```

Add the following content (adjust `Exec` path to match your publish location):

```ini
[Desktop Entry]
Name=Build Log Dashboard
Comment=Android OS Image Build Log Dashboard
Exec="/path/to/BuildLogDashboard/publish/BuildLogDashboard"
Icon=utilities-terminal
Terminal=false
Type=Application
Categories=Development;Utility;
StartupWMClass=BuildLogDashboard
```

## User Manual

### Getting Started

#### 1. Open a Workspace
Click **Open Folder** in the toolbar and select the directory containing your Android Image files (`.zip` and `.json` files).

The application will:
- Scan for existing build log files (`BUILD_LOG_*.md`, `README*.md`)
- Detect Android Image files and parse their filenames
- Auto-populate build number, device, and date from filenames
- Populate the build list in the sidebar

#### 2. Create a New Build Log
Click **New Build** to create a fresh build log entry. The application will:
- Auto-populate with current date
- Generate a temporary build number
- Add default test entries (Boot Test, Basic Functionality, OTA Update Test)

#### 3. Edit Build Information

**Build Info Tab:**
- Enter the build number (e.g., `AAL-AA-07009-01`)
- Set the build date using the date picker
- Select build type from dropdown (TBD/user/userdebug/eng)
- Select Android version and kernel version from dropdowns
- Add security patch level

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
Click **Preview** in the toolbar to open the rendered markdown preview panel. Click **Refresh** to update the preview with your latest changes.

#### 5. Save and Export

**Save:** Click **Save** to save the build log as `BUILD_LOG_{BuildNumber}.md` in your workspace.

**Export:** Click **Export** and choose from:
- **Export as Markdown (.md)** - Standard markdown format
- **Export as HTML (.html)** - Styled HTML document
- **Export as PDF (.pdf)** - Professional PDF with styled tables, color-coded badges, and headers/footers

Each export option opens a preview window where you can review the document and set the save location before confirming.

**Delete:** The Export menu also includes options to delete previously exported Markdown, HTML, or PDF files.

**Validation:** Before saving or exporting, the application validates all mandatory fields. If any are missing, an Apple-style popup dialog lists the incomplete fields.

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
├── publish/                        # Published standalone executable
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
        │   ├── MainWindow.axaml.cs # Code-behind
        │   ├── AlertDialog.axaml   # Apple-style validation popup
        │   ├── AlertDialog.axaml.cs
        │   ├── ExportPreviewWindow.axaml   # Export preview dialog
        │   └── ExportPreviewWindow.axaml.cs
        │
        ├── Services/               # Business logic
        │   ├── FileScanner.cs      # Auto-detect image files
        │   ├── MarkdownGenerator.cs # Generate markdown output
        │   ├── MarkdownParser.cs   # Parse existing markdown files
        │   ├── HtmlGenerator.cs    # Generate HTML output
        │   ├── PdfGenerator.cs     # Generate PDF output (QuestPDF)
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
| Markdown Processing | Markdig |
| Markdown Preview | Markdown.Avalonia |
| PDF Generation | QuestPDF |
| Graphics | SkiaSharp |
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

## Customer Release Notes
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
- [Markdown.Avalonia](https://github.com/whistyun/Markdown.Avalonia) - Markdown rendering for Avalonia
- [QuestPDF](https://www.questpdf.com/) - PDF document generation
- [SkiaSharp](https://github.com/mono/SkiaSharp) - Cross-platform 2D graphics

---

**Built for AOSP Build Engineers** - Simplifying Android OS Image documentation management.

DevC: **[Sachin.R.Dsilva](https://github.com/sachindsilvaNIST)**
