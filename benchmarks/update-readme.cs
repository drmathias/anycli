// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

var repoRoot = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Directory.GetCurrentDirectory();

var readmePath = Path.Combine(repoRoot, "README.md");
var resultsDirectory = Path.Combine(repoRoot, "benchmarks", "results", "latest");

var scenarios = new (string ClassName, string Title, bool IncludeAllocation)[]
{
    ("InvokeTypicalBenchmarks", "Invocation", true),
    ("ColdStartJitBenchmarks", "Cold start (JIT)", false),
    ("ColdStartAotBenchmarks", "Cold start (NativeAOT)", false),
};

var frameworkNames = new Dictionary<string, string>(StringComparer.Ordinal)
{
    ["AnyCli"] = "AnyCli",
    ["SystemCommandLine"] = "System.CommandLine",
    ["SpectreConsoleCli"] = "Spectre.Console.Cli",
    ["ConsoleAppFramework"] = "ConsoleAppFramework",
};

var readme = File.ReadAllText(readmePath);
const string startMarker = "<!-- benchmarks:start -->";
const string endMarker = "<!-- benchmarks:end -->";

var startIndex = readme.IndexOf(startMarker, StringComparison.Ordinal);
var endIndex = readme.IndexOf(endMarker, StringComparison.Ordinal);
if (startIndex < 0 || endIndex < startIndex)
{
    throw new InvalidOperationException("README benchmark markers were not found.");
}

startIndex += startMarker.Length;
var replacement = $"\n{BuildSection()}\n";
readme = string.Concat(readme.AsSpan(0, startIndex), replacement, readme.AsSpan(endIndex));
File.WriteAllText(readmePath, readme, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

string BuildSection()
{
    var lines = new List<string>();
    var environment = ReadEnvironment();
    if (environment is not null) lines.Add($"_Environment: {environment}._");

    foreach (var (className, title, includeAllocation) in scenarios)
    {
        var reportPath = Path.Combine(resultsDirectory, $"AnyCli.Benchmarks.{className}-report.csv");

        if (!File.Exists(reportPath)) continue;

        var rows = ReadRows(reportPath);
        if (rows.Count == 0) continue;

        lines.Add(string.Empty);
        lines.Add($"### {title}");
        lines.Add(string.Empty);
        lines.AddRange(FormatTable(rows, includeAllocation));
    }

    if (lines.Count == 0) lines.Add("_No benchmark results are available yet._");
    else
    {
        lines.Add(string.Empty);
        lines.Add("_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._");
    }

    return string.Join('\n', lines);
}

string? ReadEnvironment()
{
    var report = Directory
        .GetFiles(resultsDirectory, "*-report-github.md")
        .OrderBy(path => path, StringComparer.Ordinal)
        .FirstOrDefault();

    if (report is null) return null;

    string? osName = null;
    string? cpu = null;
    string? cores = null;
    string? sdk = null;
    string? runtime = null;
    string? architecture = null;

    foreach (var rawLine in File.ReadLines(report))
    {
        var line = rawLine.Trim();
        if (line.StartsWith("BenchmarkDotNet v", StringComparison.Ordinal))
        {
            var separatorIndex = line.IndexOf(", ", StringComparison.Ordinal);
            var environment = separatorIndex >= 0 ? line[(separatorIndex + 2)..] : line;
            var parenthesisIndex = environment.IndexOf(" (", StringComparison.Ordinal);
            osName = parenthesisIndex >= 0 ? environment[..parenthesisIndex] : environment;
        }
        else if (line.Contains("logical", StringComparison.Ordinal) &&
                 line.Contains("CPU", StringComparison.Ordinal))
        {
            var commaIndex = line.IndexOf(',');
            var cpuPart = commaIndex >= 0 ? line[..commaIndex] : line;
            var cpuIndex = cpuPart.IndexOf(" CPU", StringComparison.OrdinalIgnoreCase);
            cpu = (cpuIndex >= 0 ? cpuPart[..cpuIndex] : cpuPart).Trim();

            var coreMatch = Regex.Match(line, @"(\d+)\s+logical");
            if (coreMatch.Success) cores = $"{coreMatch.Groups[1].Value} logical cores";
        }
        else if (line.StartsWith(".NET SDK ", StringComparison.Ordinal)) sdk = line;
        else if (line.Contains("[Host]", StringComparison.Ordinal))
        {
            var runtimeMatch = Regex.Match(line, @":\s*(\.NET\s+[^,\s]+)");
            if (runtimeMatch.Success) runtime = runtimeMatch.Groups[1].Value;

            var architectureMatch = Regex.Match(line, @"\b(X64|X86|Arm64)\b", RegexOptions.IgnoreCase);

            if (architectureMatch.Success) architecture = architectureMatch.Groups[1].Value;
        }
    }

    var parts = new[] { osName, cpu, cores, sdk, runtime, architecture }
        .Where(part => !string.IsNullOrWhiteSpace(part))
        .ToArray();

    return parts.Length == 0 ? null : string.Join(", ", parts);
}

List<Dictionary<string, string>> ReadRows(string reportPath)
{
    var lines = File.ReadAllLines(reportPath);
    if (lines.Length == 0) return [];

    var headers = ParseCsvLine(lines[0].TrimStart('\uFEFF'));
    var rows = new List<Dictionary<string, string>>();

    foreach (var line in lines.Skip(1))
    {
        if (string.IsNullOrWhiteSpace(line)) continue;

        var fields = ParseCsvLine(line);
        var row = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < headers.Count && index < fields.Count; index++) row[headers[index]] = fields[index];

        rows.Add(row);
    }

    rows = [.. rows.Where(row => row.TryGetValue("Job", out var job) && job != "Dry")];

    if (rows.Count == 0) return [];

    var selectedJob = rows[0]["Job"];
    return rows.Where(row => row["Job"] == selectedJob).ToList();
}

List<string> FormatTable(List<Dictionary<string, string>> rows, bool includeAllocation)
{
    var sortedRows = rows
        .OrderBy(row => int.Parse(row["Rank"], CultureInfo.InvariantCulture))
        .ThenBy(row => ParseDuration(row["Mean"]))
        .ToList();

    List<string> lines = includeAllocation
        ?
        [
            "| Rank | Framework | Mean | Median | Ratio | Allocated |",
            "| ---: | --- | ---: | ---: | ---: | ---: |",
        ]
        :
        [
            "| Rank | Framework | Mean | Median | Ratio |",
            "| ---: | --- | ---: | ---: | ---: |",
        ];

    foreach (var row in sortedRows)
    {
        var method = frameworkNames.GetValueOrDefault(row["Method"], row["Method"]);
        var fields = new List<string>
        {
            row["Rank"],
            method,
            row["Mean"],
            row["Median"],
            row["Ratio"],
        };

        if (includeAllocation) fields.Add(row["Allocated"]);

        if (row["Method"] == "AnyCli") fields = [.. fields.Select(field => $"**{field}**")];

        lines.Add("| " + string.Join(" | ", fields) + " |");
    }

    return lines;
}

static double ParseDuration(string value)
{
    var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var number = double.Parse(parts[0].Replace(",", string.Empty), CultureInfo.InvariantCulture);
    var multiplier = parts[1] switch
    {
        "ns" => 1.0,
        "us" => 1_000.0,
        "ms" => 1_000_000.0,
        "s" => 1_000_000_000.0,
        _ => throw new InvalidOperationException($"Unknown duration unit '{parts[1]}'."),
    };

    return number * multiplier;
}

static List<string> ParseCsvLine(string line)
{
    var fields = new List<string>();
    var builder = new StringBuilder();
    var inQuotes = false;

    for (var index = 0; index < line.Length; index++)
    {
        var character = line[index];
        if (character == '"')
        {
            if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
            {
                builder.Append('"');
                index++;
            }
            else inQuotes = !inQuotes;
        }
        else if (character == ',' && !inQuotes)
        {
            fields.Add(builder.ToString());
            builder.Clear();
        }
        else builder.Append(character);
    }

    fields.Add(builder.ToString());
    return fields;
}
