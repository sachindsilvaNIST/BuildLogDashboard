using System.Text;
using BuildLogDashboard.Models;
using Markdig;

namespace BuildLogDashboard.Services;

public class HtmlGenerator
{
    private readonly MarkdownGenerator _markdownGenerator;
    private readonly MarkdownPipeline _pipeline;

    public HtmlGenerator(MarkdownGenerator markdownGenerator)
    {
        _markdownGenerator = markdownGenerator;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public string Generate(BuildProject project)
    {
        var markdown = _markdownGenerator.Generate(project);
        var htmlBody = Markdig.Markdown.ToHtml(markdown, _pipeline);

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>Build Log - {project.BuildNumber}</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine(GetCssStyles());
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine("        <div class=\"document\">");
        sb.AppendLine(htmlBody);
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string GetCssStyles()
    {
        return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            font-size: 14px;
            line-height: 1.6;
            color: #1a1a1a;
            background-color: #f5f5f5;
        }

        .container {
            max-width: 900px;
            margin: 40px auto;
            padding: 0 20px;
        }

        .document {
            background: white;
            padding: 60px 80px;
            border-radius: 8px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
        }

        h1 {
            font-size: 28px;
            font-weight: 600;
            color: #007AFF;
            margin-bottom: 32px;
            padding-bottom: 16px;
            border-bottom: 3px solid #007AFF;
        }

        h2 {
            font-size: 20px;
            font-weight: 600;
            color: #333;
            margin-top: 32px;
            margin-bottom: 16px;
            padding-bottom: 8px;
            border-bottom: 1px solid #e0e0e0;
        }

        h3 {
            font-size: 16px;
            font-weight: 600;
            color: #444;
            margin-top: 24px;
            margin-bottom: 12px;
        }

        h4 {
            font-size: 14px;
            font-weight: 600;
            color: #555;
            margin-top: 16px;
            margin-bottom: 8px;
        }

        p {
            margin-bottom: 12px;
        }

        ul, ol {
            margin-bottom: 16px;
            padding-left: 24px;
        }

        li {
            margin-bottom: 6px;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 24px;
            font-size: 13px;
        }

        th, td {
            padding: 12px 16px;
            text-align: left;
            border: 1px solid #e0e0e0;
        }

        th {
            background-color: #f8f9fa;
            font-weight: 600;
            color: #333;
        }

        tr:nth-child(even) {
            background-color: #fafafa;
        }

        tr:hover {
            background-color: #f0f7ff;
        }

        code {
            font-family: 'SF Mono', Monaco, 'Cascadia Code', Consolas, monospace;
            font-size: 12px;
            background-color: #f4f4f4;
            padding: 2px 6px;
            border-radius: 4px;
            color: #d63384;
        }

        pre {
            background-color: #f8f9fa;
            padding: 16px;
            border-radius: 6px;
            overflow-x: auto;
            margin-bottom: 16px;
        }

        pre code {
            background: none;
            padding: 0;
            color: inherit;
        }

        strong {
            font-weight: 600;
        }

        hr {
            border: none;
            border-top: 1px solid #e0e0e0;
            margin: 32px 0;
        }

        em {
            font-style: italic;
            color: #666;
        }

        /* Status indicators */
        td:contains('Pass'), td:contains('✅') {
            color: #107C10;
        }

        td:contains('Fail'), td:contains('❌') {
            color: #D13438;
        }

        td:contains('Pending'), td:contains('⏳') {
            color: #FF8C00;
        }

        /* Checkbox styling */
        input[type='checkbox'] {
            margin-right: 8px;
        }

        /* Print styles */
        @media print {
            body {
                background: white;
            }

            .container {
                margin: 0;
                padding: 0;
            }

            .document {
                box-shadow: none;
                padding: 20px;
            }
        }

        @page {
            margin: 2cm;
        }";
    }
}
