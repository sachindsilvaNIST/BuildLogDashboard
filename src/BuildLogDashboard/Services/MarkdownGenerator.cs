using System;
using System.Linq;
using System.Text;
using BuildLogDashboard.Models;

namespace BuildLogDashboard.Services;

public class MarkdownGenerator
{
    public string Generate(BuildProject project)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"# Android OS Image Build Log - {project.BuildNumber}");
        sb.AppendLine();

        // Build Information
        sb.AppendLine("## Build Information");
        sb.AppendLine();
        sb.AppendLine("| Property | Value |");
        sb.AppendLine("|----------|-------|");
        sb.AppendLine($"| **Build Number** | `{project.BuildNumber}` |");
        sb.AppendLine($"| **Build Date** | {project.BuildDate:yyyy-MM-dd} |");
        sb.AppendLine($"| **Device** | {project.Device} |");
        sb.AppendLine($"| **Build Type** | {project.BuildType} |");
        sb.AppendLine($"| **Android Version** | {project.AndroidVersion} |");
        sb.AppendLine($"| **Security Patch** | {project.SecurityPatch} |");
        sb.AppendLine($"| **Kernel Version** | {project.KernelVersion} |");
        sb.AppendLine($"| **Previous Build** | {project.PreviousBuild} |");
        sb.AppendLine();

        // Files
        sb.AppendLine("## Files");
        sb.AppendLine();
        sb.AppendLine("| File | Size | SHA256 |");
        sb.AppendLine("|------|------|--------|");
        foreach (var file in project.Files)
        {
            sb.AppendLine($"| `{file.FileName}` | {file.FileSize} | `{file.Sha256}` |");
        }
        sb.AppendLine();

        // Changelog
        sb.AppendLine("## Changelog");
        sb.AppendLine();

        // App Updates
        if (project.AppUpdates.Count > 0)
        {
            sb.AppendLine("### App Updates");
            sb.AppendLine();
            sb.AppendLine("| App | Path | Version | Changes | Description |");
            sb.AppendLine("|-----|------|---------|---------|-------------|");
            foreach (var app in project.AppUpdates)
            {
                sb.AppendLine($"| {app.AppName} | `{app.Path}` | {app.Version} | {app.Changes} | {app.Description} |");
            }
            sb.AppendLine();

            // App Details
            foreach (var app in project.AppUpdates.Where(a => a.Details.Count > 0))
            {
                sb.AppendLine($"#### {app.AppName} Details");
                sb.AppendLine();
                foreach (var detail in app.Details)
                {
                    sb.AppendLine($"- {detail}");
                }
                sb.AppendLine();
            }
        }

        // System Modifications
        if (!string.IsNullOrWhiteSpace(project.SystemModifications))
        {
            sb.AppendLine("### System Modifications");
            sb.AppendLine();
            foreach (var line in project.SystemModifications.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine($"- {line.Trim()}");
            }
            sb.AppendLine();
        }

        // Kernel/Driver Changes
        if (!string.IsNullOrWhiteSpace(project.KernelDriverChanges))
        {
            sb.AppendLine("### Kernel/Driver Changes");
            sb.AppendLine();
            foreach (var line in project.KernelDriverChanges.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine($"- {line.Trim()}");
            }
            sb.AppendLine();
        }

        // Configuration Changes
        if (!string.IsNullOrWhiteSpace(project.ConfigurationChanges))
        {
            sb.AppendLine("### Configuration Changes");
            sb.AppendLine();
            foreach (var line in project.ConfigurationChanges.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine($"- {line.Trim()}");
            }
            sb.AppendLine();
        }

        // Removed Components
        if (!string.IsNullOrWhiteSpace(project.RemovedComponents))
        {
            sb.AppendLine("### Removed Components");
            sb.AppendLine();
            foreach (var line in project.RemovedComponents.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                sb.AppendLine($"- {line.Trim()}");
            }
            sb.AppendLine();
        }

        // Known Issues
        if (project.KnownIssues.Count > 0)
        {
            sb.AppendLine("## Known Issues");
            sb.AppendLine();
            sb.AppendLine("| Issue | Severity | Status | Workaround |");
            sb.AppendLine("|-------|----------|--------|------------|");
            foreach (var issue in project.KnownIssues)
            {
                sb.AppendLine($"| {issue.Issue} | {issue.Severity} | {issue.Status} | {issue.Workaround} |");
            }
            sb.AppendLine();
        }

        // Testing Status
        if (project.TestResults.Count > 0)
        {
            sb.AppendLine("## Testing Status");
            sb.AppendLine();
            sb.AppendLine("| Test | Result | Notes |");
            sb.AppendLine("|------|--------|-------|");
            foreach (var test in project.TestResults)
            {
                var resultEmoji = test.Result switch
                {
                    "Pass" => "✅",
                    "Fail" => "❌",
                    "Pending" => "⏳",
                    "Skipped" => "⏭️",
                    _ => "❓"
                };
                sb.AppendLine($"| {test.TestName} | {resultEmoji} {test.Result} | {test.Notes} |");
            }
            sb.AppendLine();
        }

        // Dependencies
        sb.AppendLine("## Dependencies");
        sb.AppendLine();
        sb.AppendLine($"- **Bootloader Version**: {project.BootloaderVersion}");
        sb.AppendLine($"- **Compatible OTA Builds**: {project.CompatibleOtaBuilds}");
        sb.AppendLine();

        // Recommended For
        sb.AppendLine("## Recommended For");
        sb.AppendLine();
        sb.AppendLine($"- [x] Internal Testing");
        sb.AppendLine($"- [{(project.CustomerRelease ? "x" : " ")}] Customer Release");
        if (!string.IsNullOrWhiteSpace(project.SpecificCustomer))
        {
            sb.AppendLine($"- **Specific Customer**: {project.SpecificCustomer}");
        }
        sb.AppendLine();

        // Customer Release Notes
        if (!string.IsNullOrWhiteSpace(project.CustomerReleaseNotes))
        {
            sb.AppendLine("## Customer Release Notes");
            sb.AppendLine();
            sb.AppendLine(project.CustomerReleaseNotes);
            sb.AppendLine();
        }

        // Build Engineer
        sb.AppendLine("## Build Engineer");
        sb.AppendLine();
        sb.AppendLine($"- **Built by**: {project.BuiltBy}");
        sb.AppendLine($"- **Reviewed by**: {project.ReviewedBy}");
        if (project.ApprovedForReleaseDate.HasValue)
        {
            sb.AppendLine($"- **Approved for release**: {project.ApprovedForReleaseDate.Value:yyyy-MM-dd}");
        }
        sb.AppendLine();

        // Footer
        sb.AppendLine("---");
        sb.AppendLine($"*Last updated: {project.LastUpdated:yyyy-MM-dd HH:mm:ss}*");

        return sb.ToString();
    }
}
