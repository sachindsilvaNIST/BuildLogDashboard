using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BuildLogDashboard.Models;

namespace BuildLogDashboard.Services;

public class ProjectManager
{
    private readonly FileScanner _fileScanner;
    private readonly MarkdownGenerator _markdownGenerator;
    private readonly MarkdownParser _markdownParser;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ProjectManager()
    {
        _fileScanner = new FileScanner();
        _markdownGenerator = new MarkdownGenerator();
        _markdownParser = new MarkdownParser();
    }

    public string? CurrentWorkspace { get; private set; }
    public MarkdownGenerator MarkdownGenerator => _markdownGenerator;

    public void SetWorkspace(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            CurrentWorkspace = directoryPath;
        }
    }

    public List<BuildProject> LoadAllProjects()
    {
        var projects = new List<BuildProject>();

        if (string.IsNullOrEmpty(CurrentWorkspace) || !Directory.Exists(CurrentWorkspace))
            return projects;

        // Load from README files
        var readmeFiles = Directory.GetFiles(CurrentWorkspace, "README*.md")
            .Concat(Directory.GetFiles(CurrentWorkspace, "BUILD_LOG*.md"))
            .ToList();

        foreach (var readmeFile in readmeFiles)
        {
            try
            {
                var project = _markdownParser.ParseFile(readmeFile);
                if (!string.IsNullOrEmpty(project.BuildNumber))
                {
                    // Resolve FullPath for files parsed from markdown
                    foreach (var file in project.Files)
                    {
                        if (string.IsNullOrEmpty(file.FullPath) && !string.IsNullOrEmpty(file.FileName))
                        {
                            var candidatePath = Path.Combine(CurrentWorkspace, file.FileName);
                            if (File.Exists(candidatePath))
                            {
                                file.FullPath = candidatePath;
                            }
                        }
                    }
                    projects.Add(project);
                }
            }
            catch (Exception)
            {
                // Skip files that can't be parsed
            }
        }

        // Also check for builds that don't have README files yet
        var buildIdentifiers = _fileScanner.GetUniqueBuildIdentifiers(CurrentWorkspace);
        var existingBuildNumbers = projects.Select(p => p.BuildNumber).ToHashSet();

        foreach (var identifier in buildIdentifiers)
        {
            // Check if this full identifier already exists (format: buildnum.date.time)
            if (!existingBuildNumbers.Contains(identifier))
            {
                var project = _fileScanner.CreateProjectFromFiles(CurrentWorkspace, identifier);
                if (!string.IsNullOrEmpty(project.BuildNumber))
                {
                    projects.Add(project);
                }
            }
        }

        return projects.OrderByDescending(p => p.BuildDate).ToList();
    }

    public BuildProject CreateNewProject()
    {
        var project = new BuildProject
        {
            BuildDate = DateTime.Now,
            LastUpdated = DateTime.Now
        };

        if (!string.IsNullOrEmpty(CurrentWorkspace))
        {
            // Auto-scan for files
            var files = _fileScanner.ScanDirectory(CurrentWorkspace);
            foreach (var file in files)
            {
                project.Files.Add(file);
            }
        }

        return project;
    }

    public BuildProject? LoadProject(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        return _markdownParser.ParseFile(filePath);
    }

    public async Task SaveProjectAsync(BuildProject project)
    {
        if (string.IsNullOrEmpty(CurrentWorkspace))
            return;

        project.LastUpdated = DateTime.Now;

        // Generate filename based on build number
        var fileName = string.IsNullOrEmpty(project.BuildNumber)
            ? $"BUILD_LOG_{project.BuildDate:yyyyMMdd_HHmmss}.md"
            : $"BUILD_LOG_{project.BuildNumber}.md";

        var filePath = Path.Combine(CurrentWorkspace, fileName);
        project.ProjectFilePath = filePath;

        var markdown = _markdownGenerator.Generate(project);
        await File.WriteAllTextAsync(filePath, markdown);
    }

    public async Task ExportAsMarkdownAsync(BuildProject project, string filePath)
    {
        project.LastUpdated = DateTime.Now;
        var markdown = _markdownGenerator.Generate(project);
        await File.WriteAllTextAsync(filePath, markdown);
    }

    public string GeneratePreview(BuildProject project)
    {
        return _markdownGenerator.Generate(project);
    }

    public async Task<BuildProject> ImportMarkdownAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Markdown file not found", filePath);

        var content = await File.ReadAllTextAsync(filePath);
        var project = _markdownParser.Parse(content);
        project.ProjectFilePath = filePath;

        return project;
    }

    public async Task ComputeFileChecksumsAsync(BuildProject project)
    {
        foreach (var file in project.Files)
        {
            if (!string.IsNullOrEmpty(file.FullPath) && File.Exists(file.FullPath))
            {
                file.Sha256 = await _fileScanner.ComputeSha256Async(file.FullPath);
            }
        }
    }
}
