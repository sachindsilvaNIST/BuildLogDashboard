using System;
using System.Collections.Generic;
using System.Linq;
using BuildLogDashboard.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BuildLogDashboard.Services;

public class PdfGenerator
{
    // Colors
    private static readonly string HeadingColor = "#01548c";  // Blue for side headings
    private static readonly string MainHeadingColor = "#888888";  // Faded gray for main heading
    private static readonly string TextColor = "#1A1A1A";
    private static readonly string SubtleColor = "#666666";
    private static readonly string BorderColor = "#E0E0E0";
    private static readonly string SuccessColor = "#107C10";
    private static readonly string ErrorColor = "#D13438";
    private static readonly string WarningColor = "#FF8C00";

    static PdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private IDocument CreateDocument(BuildProject project)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextColor).FontFamily("Inter"));

                page.Header().Element(c => ComposeHeader(c, project));
                page.Content().Element(c => ComposeContent(c, project));
                page.Footer().Element(c => ComposeFooter(c, project));
            });
        });
    }

    public void Generate(BuildProject project, string filePath)
    {
        try
        {
            CreateDocument(project).GeneratePdf(filePath);
        }
        catch (Exception ex)
        {
            throw new Exception($"PDF generation error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates preview images of the PDF pages
    /// </summary>
    /// <param name="project">The build project to preview</param>
    /// <param name="dpi">Resolution for the preview images (default 150 for good balance of quality/performance)</param>
    /// <returns>List of PNG image bytes for each page</returns>
    public List<byte[]> GeneratePreviewImages(BuildProject project, int dpi = 150)
    {
        try
        {
            var images = new List<byte[]>();
            var document = CreateDocument(project);

            foreach (var imageBytes in document.GenerateImages(new ImageGenerationSettings { ImageFormat = ImageFormat.Png, RasterDpi = dpi }))
            {
                images.Add(imageBytes);
            }

            return images;
        }
        catch (Exception ex)
        {
            throw new Exception($"PDF preview generation error: {ex.Message}", ex);
        }
    }

    private void ComposeHeader(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text($"Android OS Image Build Log").FontSize(14).Bold().FontColor(MainHeadingColor);
            column.Item().AlignCenter().Text($"Build: {project.BuildNumber}").FontSize(11).FontColor(SubtleColor);
            column.Item().PaddingTop(10).LineHorizontal(2).LineColor(HeadingColor);
        });
    }

    private void ComposeContent(IContainer container, BuildProject project)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Build Information Section
            column.Item().Element(c => ComposeBuildInfo(c, project));

            // Files Section
            if (project.Files.Count > 0)
                column.Item().Element(c => ComposeFiles(c, project));

            // Changelog Section
            column.Item().Element(c => ComposeChangelog(c, project));

            // Known Issues Section
            if (project.KnownIssues.Count > 0)
                column.Item().Element(c => ComposeKnownIssues(c, project));

            // Testing Status Section
            if (project.TestResults.Count > 0)
                column.Item().Element(c => ComposeTestResults(c, project));

            // Dependencies Section
            column.Item().Element(c => ComposeDependencies(c, project));

            // Recommended For Section
            column.Item().Element(c => ComposeRecommendedFor(c, project));

            // Customer Release Notes
            if (!string.IsNullOrWhiteSpace(project.CustomerReleaseNotes))
                column.Item().Element(c => ComposeCustomerNotes(c, project));

            // Build Engineer Section
            column.Item().Element(c => ComposeBuildEngineer(c, project));
        });
    }

    private void ComposeBuildInfo(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Build Information").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                AddTableRow(table, "Build Number", project.BuildNumber, "Build Date", project.BuildDate.ToString("yyyy-MM-dd"));
                AddTableRow(table, "Device", project.Device, "Build Type", project.BuildType);
                AddTableRow(table, "Android Version", project.AndroidVersion, "Security Patch", project.SecurityPatch);
                AddTableRow(table, "Kernel Version", project.KernelVersion, "Previous Build", project.PreviousBuild);
            });
        });
    }

    private void AddTableRow(TableDescriptor table, string label1, string value1, string label2, string value2)
    {
        table.Cell().PaddingVertical(4).Text(label1).Bold().FontColor(SubtleColor);
        table.Cell().PaddingVertical(4).Text(value1 ?? "-");
        table.Cell().PaddingVertical(4).Text(label2).Bold().FontColor(SubtleColor);
        table.Cell().PaddingVertical(4).Text(value2 ?? "-");
    }

    private void ComposeFiles(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Files").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(3);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background("#F8F9FA").Padding(6).Text("File Name").Bold();
                    header.Cell().Background("#F8F9FA").Padding(6).Text("Size").Bold();
                    header.Cell().Background("#F8F9FA").Padding(6).Text("SHA256").Bold();
                });

                foreach (var file in project.Files)
                {
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(6).Text(file.FileName).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(6).Text(file.FileSize ?? "-");
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(6).Text(file.Sha256 ?? "-").FontSize(8);
                }
            });
        });
    }

    private void ComposeChangelog(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Changelog").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);

            // App Updates
            if (project.AppUpdates.Count > 0)
            {
                column.Item().PaddingTop(10).Text("App Updates").FontSize(12).SemiBold();
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#F8F9FA").Padding(4).Text("App").Bold().FontSize(9);
                        header.Cell().Background("#F8F9FA").Padding(4).Text("Path").Bold().FontSize(9);
                        header.Cell().Background("#F8F9FA").Padding(4).Text("Version").Bold().FontSize(9);
                        header.Cell().Background("#F8F9FA").Padding(4).Text("Changes").Bold().FontSize(9);
                    });

                    foreach (var app in project.AppUpdates)
                    {
                        table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(app.AppName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(app.Path).FontSize(8);
                        table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(app.Version).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(app.Changes).FontSize(9);
                    }
                });
            }

            // System Modifications
            if (!string.IsNullOrWhiteSpace(project.SystemModifications))
            {
                column.Item().PaddingTop(10).Text("System Modifications").FontSize(12).SemiBold();
                foreach (var line in project.SystemModifications.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    column.Item().PaddingLeft(10).Text($"• {line.Trim()}").FontSize(9);
                }
            }

            // Kernel/Driver Changes
            if (!string.IsNullOrWhiteSpace(project.KernelDriverChanges))
            {
                column.Item().PaddingTop(10).Text("Kernel/Driver Changes").FontSize(12).SemiBold();
                foreach (var line in project.KernelDriverChanges.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    column.Item().PaddingLeft(10).Text($"• {line.Trim()}").FontSize(9);
                }
            }

            // Configuration Changes
            if (!string.IsNullOrWhiteSpace(project.ConfigurationChanges))
            {
                column.Item().PaddingTop(10).Text("Configuration Changes").FontSize(12).SemiBold();
                foreach (var line in project.ConfigurationChanges.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    column.Item().PaddingLeft(10).Text($"• {line.Trim()}").FontSize(9);
                }
            }

            // Removed Components
            if (!string.IsNullOrWhiteSpace(project.RemovedComponents))
            {
                column.Item().PaddingTop(10).Text("Removed Components").FontSize(12).SemiBold();
                foreach (var line in project.RemovedComponents.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    column.Item().PaddingLeft(10).Text($"• {line.Trim()}").FontSize(9);
                }
            }
        });
    }

    private void ComposeKnownIssues(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Known Issues").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Issue").Bold().FontSize(9);
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Severity").Bold().FontSize(9);
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Status").Bold().FontSize(9);
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Workaround").Bold().FontSize(9);
                });

                foreach (var issue in project.KnownIssues)
                {
                    var severityColor = issue.Severity switch
                    {
                        "Critical" => ErrorColor,
                        "High" => ErrorColor,
                        "Medium" => WarningColor,
                        _ => TextColor
                    };

                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(issue.Issue).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(issue.Severity).FontSize(9).FontColor(severityColor);
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(issue.Status).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(issue.Workaround).FontSize(9);
                }
            });
        });
    }

    private void ComposeTestResults(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Testing Status").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(3);
                });

                table.Header(header =>
                {
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Test").Bold().FontSize(9);
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Result").Bold().FontSize(9);
                    header.Cell().Background("#F8F9FA").Padding(4).Text("Notes").Bold().FontSize(9);
                });

                foreach (var test in project.TestResults)
                {
                    var resultColor = test.Result switch
                    {
                        "Pass" => SuccessColor,
                        "Fail" => ErrorColor,
                        "Pending" => WarningColor,
                        _ => TextColor
                    };

                    var resultIcon = test.Result switch
                    {
                        "Pass" => "PASS",
                        "Fail" => "FAIL",
                        "Pending" => "PENDING",
                        "Skipped" => "SKIPPED",
                        _ => test.Result
                    };

                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(test.TestName).FontSize(9);
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(resultIcon).FontSize(9).FontColor(resultColor).Bold();
                    table.Cell().BorderBottom(1).BorderColor(BorderColor).Padding(4).Text(test.Notes).FontSize(9);
                }
            });
        });
    }

    private void ComposeDependencies(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Dependencies").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Bootloader Version").Bold().FontColor(SubtleColor);
                    c.Item().Text(project.BootloaderVersion ?? "-");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Compatible OTA Builds").Bold().FontColor(SubtleColor);
                    c.Item().Text(project.CompatibleOtaBuilds ?? "-");
                });
            });
        });
    }

    private void ComposeRecommendedFor(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Recommended For").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Column(c =>
            {
                c.Item().Text(text =>
                {
                    text.Span(project.InternalTesting ? "\u2713 " : "\u2717 ").FontColor(project.InternalTesting ? SuccessColor : ErrorColor);
                    text.Span("Internal Testing");
                });
                c.Item().Text(text =>
                {
                    text.Span(project.CustomerRelease ? "\u2713 " : "\u2717 ").FontColor(project.CustomerRelease ? SuccessColor : ErrorColor);
                    text.Span("Customer Release");
                });
                if (!string.IsNullOrWhiteSpace(project.SpecificCustomer))
                {
                    c.Item().PaddingTop(5).Text($"Specific Customer: {project.SpecificCustomer}").FontColor(SubtleColor);
                }
            });
        });
    }

    private void ComposeCustomerNotes(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Customer Release Notes").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Text(project.CustomerReleaseNotes);
        });
    }

    private void ComposeBuildEngineer(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().Text("Build Engineer").FontSize(11).Bold().FontColor(HeadingColor);
            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                table.Cell().PaddingVertical(4).Text("Built by").Bold().FontColor(SubtleColor);
                table.Cell().PaddingVertical(4).Text(project.BuiltBy ?? "-");

                table.Cell().PaddingVertical(4).Text("Reviewed by").Bold().FontColor(SubtleColor);
                table.Cell().PaddingVertical(4).Text(project.ReviewedBy ?? "-");

                if (project.ApprovedForReleaseDate.HasValue)
                {
                    table.Cell().PaddingVertical(4).Text("Approved Date").Bold().FontColor(SubtleColor);
                    table.Cell().PaddingVertical(4).Text(project.ApprovedForReleaseDate.Value.ToString("yyyy-MM-dd"));
                }
            });
        });
    }

    private void ComposeFooter(IContainer container, BuildProject project)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(BorderColor);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text($"Last updated: {project.LastUpdated:yyyy-MM-dd HH:mm:ss}").FontSize(8).FontColor(SubtleColor);
                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(8).FontColor(SubtleColor);
                    text.CurrentPageNumber().FontSize(8).FontColor(SubtleColor);
                    text.Span(" of ").FontSize(8).FontColor(SubtleColor);
                    text.TotalPages().FontSize(8).FontColor(SubtleColor);
                });
            });
        });
    }
}
