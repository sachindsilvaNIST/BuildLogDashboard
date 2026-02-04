using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BuildLogDashboard.Models;

namespace BuildLogDashboard.Services;

public class FileScanner
{
    // Pattern: {device}-{buildnum}.{date}.{time}.{ext}
    // Example: gpn600_001-AAL-AA-07009-01.20260130.062740.zip
    // Device is before first dash, buildnum is everything between first dash and date
    private static readonly Regex FileNamePattern = new(
        @"^(?<device>[^-]+)-(?<buildnum>.+?)\.(?<date>\d{8})\.(?<time>\d+)\.(?<ext>zip|json)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public List<BuildFile> ScanDirectory(string directoryPath)
    {
        var files = new List<BuildFile>();

        if (!Directory.Exists(directoryPath))
            return files;

        var relevantFiles = Directory.GetFiles(directoryPath)
            .Where(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                       f.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var filePath in relevantFiles)
        {
            var fileInfo = new FileInfo(filePath);
            var buildFile = new BuildFile
            {
                FileName = fileInfo.Name,
                FileSize = FormatFileSize(fileInfo.Length),
                FullPath = filePath,
                Sha256 = "-"
            };
            files.Add(buildFile);
        }

        return files;
    }

    public async Task<string> ComputeSha256Async(string filePath)
    {
        if (!File.Exists(filePath))
            return "-";

        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hash = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public (string device, string buildNumber, string date, string time)? ParseFileName(string fileName)
    {
        var match = FileNamePattern.Match(fileName);
        if (!match.Success)
            return null;

        return (
            match.Groups["device"].Value,
            match.Groups["buildnum"].Value,
            match.Groups["date"].Value,
            match.Groups["time"].Value
        );
    }

    public List<string> GetUniqueBuildIdentifiers(string directoryPath)
    {
        var identifiers = new HashSet<string>();

        if (!Directory.Exists(directoryPath))
            return identifiers.ToList();

        var files = Directory.GetFiles(directoryPath)
            .Where(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                       f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var parsed = ParseFileName(fileName);
            if (parsed.HasValue)
            {
                var identifier = $"{parsed.Value.buildNumber}.{parsed.Value.date}.{parsed.Value.time}";
                identifiers.Add(identifier);
            }
        }

        return identifiers.OrderByDescending(x => x).ToList();
    }

    public BuildProject CreateProjectFromFiles(string directoryPath, string buildIdentifier)
    {
        var project = new BuildProject();
        var files = ScanDirectory(directoryPath);

        // Filter files matching the build identifier
        var matchingFiles = files.Where(f =>
        {
            var parsed = ParseFileName(f.FileName);
            if (!parsed.HasValue) return false;
            var identifier = $"{parsed.Value.buildNumber}.{parsed.Value.date}.{parsed.Value.time}";
            return identifier == buildIdentifier;
        }).ToList();

        if (matchingFiles.Count > 0)
        {
            var firstFile = matchingFiles[0];
            var parsed = ParseFileName(firstFile.FileName);
            if (parsed.HasValue)
            {
                // Transform device: gpn600_001 -> GPN600-001 (uppercase, underscore to dash)
                project.Device = parsed.Value.device.Replace("_", "-").ToUpperInvariant();
                // Set build number to include date and time: buildnum.date.time
                project.BuildNumber = $"{parsed.Value.buildNumber}.{parsed.Value.date}.{parsed.Value.time}";

                // Parse date from format YYYYMMDD
                if (DateTime.TryParseExact(parsed.Value.date, "yyyyMMdd",
                    null, System.Globalization.DateTimeStyles.None, out var buildDate))
                {
                    project.BuildDate = buildDate;
                }
            }

            foreach (var file in matchingFiles)
            {
                project.Files.Add(file);
            }
        }

        return project;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}
