using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BuildLogDashboard.Models;

namespace BuildLogDashboard.Services;

public class MarkdownParser
{
    public BuildProject Parse(string markdownContent)
    {
        var project = new BuildProject();
        project.TestResults.Clear(); // Clear default test results

        var lines = markdownContent.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        var currentSection = "";
        var currentSubSection = "";

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            // Track sections
            if (line.StartsWith("## "))
            {
                currentSection = line[3..].Trim();
                currentSubSection = "";
                continue;
            }
            if (line.StartsWith("### "))
            {
                currentSubSection = line[4..].Trim();
                continue;
            }

            // Parse Build Information table
            if (currentSection == "Build Information" && line.StartsWith("|") && line.Contains("|"))
            {
                ParseBuildInfoRow(line, project);
            }

            // Parse Files table
            if (currentSection == "Files" && line.StartsWith("|") && !line.Contains("---"))
            {
                ParseFileRow(line, project);
            }

            // Parse App Updates table
            if (currentSection == "Changelog" && currentSubSection == "App Updates" &&
                line.StartsWith("|") && !line.Contains("---"))
            {
                ParseAppUpdateRow(line, project);
            }

            // Parse System Modifications
            if (currentSection == "Changelog" && currentSubSection == "System Modifications" && line.StartsWith("- "))
            {
                project.SystemModifications += line[2..].Trim() + "\n";
            }

            // Parse Kernel/Driver Changes
            if (currentSection == "Changelog" && currentSubSection == "Kernel/Driver Changes" && line.StartsWith("- "))
            {
                project.KernelDriverChanges += line[2..].Trim() + "\n";
            }

            // Parse Configuration Changes
            if (currentSection == "Changelog" && currentSubSection == "Configuration Changes" && line.StartsWith("- "))
            {
                project.ConfigurationChanges += line[2..].Trim() + "\n";
            }

            // Parse Removed Components
            if (currentSection == "Changelog" && currentSubSection == "Removed Components" && line.StartsWith("- "))
            {
                project.RemovedComponents += line[2..].Trim() + "\n";
            }

            // Parse Known Issues table
            if (currentSection == "Known Issues" && line.StartsWith("|") && !line.Contains("---"))
            {
                ParseKnownIssueRow(line, project);
            }

            // Parse Testing Status table
            if (currentSection == "Testing Status" && line.StartsWith("|") && !line.Contains("---"))
            {
                ParseTestResultRow(line, project);
            }

            // Parse Dependencies
            if (currentSection == "Dependencies" && line.StartsWith("- "))
            {
                ParseDependencyLine(line, project);
            }

            // Parse Recommended For
            if (currentSection == "Recommended For")
            {
                ParseRecommendedForLine(line, project);
            }

            // Parse Customer Release Notes
            if (currentSection == "Customer Release Notes" && !string.IsNullOrWhiteSpace(line))
            {
                project.CustomerReleaseNotes += line + "\n";
            }

            // Parse Build Engineer
            if (currentSection == "Build Engineer" && line.StartsWith("- "))
            {
                ParseBuildEngineerLine(line, project);
            }

            // Parse Last Updated
            if (line.StartsWith("*Last updated:"))
            {
                var match = Regex.Match(line, @"\*Last updated:\s*(.+)\*");
                if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var lastUpdated))
                {
                    project.LastUpdated = lastUpdated;
                }
            }
        }

        // Trim trailing newlines
        project.SystemModifications = project.SystemModifications?.Trim() ?? "";
        project.KernelDriverChanges = project.KernelDriverChanges?.Trim() ?? "";
        project.ConfigurationChanges = project.ConfigurationChanges?.Trim() ?? "";
        project.RemovedComponents = project.RemovedComponents?.Trim() ?? "";
        project.CustomerReleaseNotes = project.CustomerReleaseNotes?.Trim() ?? "";

        return project;
    }

    public BuildProject ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
            return new BuildProject();

        var content = File.ReadAllText(filePath);
        var project = Parse(content);
        project.ProjectFilePath = filePath;
        return project;
    }

    private void ParseBuildInfoRow(string line, BuildProject project)
    {
        var cells = SplitTableRow(line);
        if (cells.Count < 2) return;

        var property = CleanCellContent(cells[0]);
        var value = CleanCellContent(cells[1]);

        switch (property.ToLower())
        {
            case "build number":
                project.BuildNumber = value;
                break;
            case "build date":
                if (DateTime.TryParse(value, out var buildDate))
                    project.BuildDate = buildDate;
                break;
            case "device":
                project.Device = value;
                break;
            case "build type":
                project.BuildType = value;
                break;
            case "android version":
                project.AndroidVersion = value;
                break;
            case "security patch":
                project.SecurityPatch = value;
                break;
            case "kernel version":
                project.KernelVersion = value;
                break;
            case "previous build":
                project.PreviousBuild = value;
                break;
        }
    }

    private void ParseFileRow(string line, BuildProject project)
    {
        var cells = SplitTableRow(line);
        if (cells.Count < 3 || cells[0].ToLower().Contains("file")) return; // Skip header

        project.Files.Add(new BuildFile
        {
            FileName = CleanCellContent(cells[0]),
            FileSize = CleanCellContent(cells[1]),
            Sha256 = CleanCellContent(cells[2])
        });
    }

    private void ParseAppUpdateRow(string line, BuildProject project)
    {
        var cells = SplitTableRow(line);
        if (cells.Count < 4 || cells[0].ToLower().Contains("app")) return; // Skip header

        var appUpdate = new AppUpdate
        {
            AppName = CleanCellContent(cells[0]),
            Path = CleanCellContent(cells[1]),
            Version = CleanCellContent(cells[2]),
            Changes = CleanCellContent(cells[3])
        };

        project.AppUpdates.Add(appUpdate);
    }

    private void ParseKnownIssueRow(string line, BuildProject project)
    {
        var cells = SplitTableRow(line);
        if (cells.Count < 4 || cells[0].ToLower().Contains("issue")) return; // Skip header

        project.KnownIssues.Add(new KnownIssue
        {
            Issue = CleanCellContent(cells[0]),
            Severity = CleanCellContent(cells[1]),
            Status = CleanCellContent(cells[2]),
            Workaround = CleanCellContent(cells[3])
        });
    }

    private void ParseTestResultRow(string line, BuildProject project)
    {
        var cells = SplitTableRow(line);
        if (cells.Count < 3 || cells[0].ToLower().Contains("test")) return; // Skip header

        // Remove emoji from result
        var result = CleanCellContent(cells[1]);
        result = Regex.Replace(result, @"[✅❌⏳⏭️❓]\s*", "").Trim();

        project.TestResults.Add(new TestResult
        {
            TestName = CleanCellContent(cells[0]),
            Result = result,
            Notes = CleanCellContent(cells[2])
        });
    }

    private void ParseDependencyLine(string line, BuildProject project)
    {
        if (line.Contains("Bootloader Version"))
        {
            var match = Regex.Match(line, @"Bootloader Version\*{0,2}:\s*(.+)$");
            if (match.Success)
                project.BootloaderVersion = match.Groups[1].Value.Trim();
        }
        else if (line.Contains("Compatible OTA"))
        {
            var match = Regex.Match(line, @"Compatible OTA[^:]*:\s*(.+)$");
            if (match.Success)
                project.CompatibleOtaBuilds = match.Groups[1].Value.Trim();
        }
    }

    private void ParseRecommendedForLine(string line, BuildProject project)
    {
        if (line.Contains("Internal Testing"))
        {
            project.InternalTesting = line.Contains("[x]");
        }
        else if (line.Contains("Customer Release") && !line.Contains("Specific"))
        {
            project.CustomerRelease = line.Contains("[x]");
        }
        else if (line.Contains("Specific Customer"))
        {
            var match = Regex.Match(line, @"Specific Customer\*{0,2}:\s*(.+)$");
            if (match.Success)
                project.SpecificCustomer = match.Groups[1].Value.Trim();
        }
    }

    private void ParseBuildEngineerLine(string line, BuildProject project)
    {
        if (line.Contains("Built by"))
        {
            var match = Regex.Match(line, @"Built by\*{0,2}:\s*(.+)$");
            if (match.Success)
                project.BuiltBy = match.Groups[1].Value.Trim();
        }
        else if (line.Contains("Reviewed by"))
        {
            var match = Regex.Match(line, @"Reviewed by\*{0,2}:\s*(.+)$");
            if (match.Success)
                project.ReviewedBy = match.Groups[1].Value.Trim();
        }
        else if (line.Contains("Approved for release"))
        {
            var match = Regex.Match(line, @"Approved for release\*{0,2}:\s*(.+)$");
            if (match.Success && DateTime.TryParse(match.Groups[1].Value.Trim(), out var approvedDate))
                project.ApprovedForReleaseDate = approvedDate;
        }
    }

    private List<string> SplitTableRow(string line)
    {
        return line.Split('|')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .ToList();
    }

    private string CleanCellContent(string content)
    {
        // Remove markdown formatting like **, `, etc.
        content = content.Trim();
        content = Regex.Replace(content, @"\*{1,2}", "");
        content = Regex.Replace(content, @"`", "");
        return content.Trim();
    }
}
