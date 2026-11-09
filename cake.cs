// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

#:sdk Cake.Sdk@6.3.0

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var versionArgument = Argument("package-version", string.Empty).Trim();

if (string.IsNullOrWhiteSpace(versionArgument))
    throw new CakeException("A version is required. Pass it with --package-version=<semver>, for example: dotnet cake.cs -- --package-version=1.2.3");

if (!SemVersion.TryParse(versionArgument, out var version))
    throw new CakeException($"Invalid version '{versionArgument}'. Specify a semantic version such as 1.2.3 or 1.2.3-beta.1.");

var versionText = version.ToString();
var solution = "./AnyCli.slnx";
var packagesDirectory = "./artifacts/packages";
var packageProjects = new[]
{
    "./src/AnyCli/AnyCli.csproj",
    "./src/AnyCli.SpectreConsole/AnyCli.SpectreConsole.csproj",
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    if (DirectoryExists("./artifacts")) CleanDirectory("./artifacts");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => DotNetRestore(solution, new DotNetRestoreSettings()));

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => DotNetBuild(solution, new DotNetBuildSettings
    {
        Configuration = configuration,
        NoRestore = true,
        MSBuildSettings = CreateVersionedMsBuildSettings(versionText),
    }));

Task("Test")
    .IsDependentOn("Build")
    .Does(() => DotNetTest(solution, new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
    }));

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    foreach (var project in packageProjects)
    {
        DotNetPack(project, new DotNetPackSettings
        {
            Configuration = configuration,
            OutputDirectory = packagesDirectory,
            NoBuild = true,
            NoRestore = true,
            MSBuildSettings = CreateVersionedMsBuildSettings(versionText),
        });
    }
});

Task("Push")
.Does(() =>
{
    var apiKey = GetConfigurationValue("NUGET_API_KEY");
    if (string.IsNullOrWhiteSpace(apiKey)) throw new CakeException("NUGET_API_KEY is required. Add it to .env or set it as an environment variable.");

    var source = GetConfigurationValue("NUGET_SOURCE");
    if (string.IsNullOrWhiteSpace(source)) source = "https://api.nuget.org/v3/index.json";

    var packages = GetFiles($"{packagesDirectory}/*.nupkg")
        .Concat(GetFiles($"{packagesDirectory}/*.snupkg"))
        .ToArray();
    if (packages.Length == 0) throw new CakeException($"No NuGet packages were found in {packagesDirectory}.");

    foreach (var package in packages)
    {
        DotNetNuGetPush(package, new DotNetNuGetPushSettings
        {
            ApiKey = apiKey,
            Source = source,
            SkipDuplicate = true,
        });
    }
});

Task("Publish")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    .IsDependentOn("Push");

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// HELPERS
//////////////////////////////////////////////////////////////////////

static DotNetMSBuildSettings CreateVersionedMsBuildSettings(string versionText)
{
    var settings = new DotNetMSBuildSettings()
        .WithProperty("Version", versionText)
        .WithProperty("ContinuousIntegrationBuild", "true");

    var repositoryUrl = GetConfigurationValue("REPOSITORY_URL");
    if (!string.IsNullOrWhiteSpace(repositoryUrl)) settings.WithProperty("RepositoryUrl", repositoryUrl);

    var packageProjectUrl = GetConfigurationValue("PACKAGE_PROJECT_URL");
    if (!string.IsNullOrWhiteSpace(packageProjectUrl)) settings.WithProperty("PackageProjectUrl", packageProjectUrl);

    return settings;
}

static string? GetConfigurationValue(string name)
{
    var environmentValue = Environment.GetEnvironmentVariable(name);
    if (!string.IsNullOrWhiteSpace(environmentValue)) return environmentValue;

    const string envFile = "./.env";
    if (!FileExists(envFile)) return null;

    foreach (var rawLine in System.IO.File.ReadAllLines(envFile))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line[0] == '#') continue;

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0) continue;

        var key = line[..separatorIndex].Trim();
        if (key.StartsWith("export ", StringComparison.Ordinal)) key = key["export ".Length..].Trim();

        if (!string.Equals(key, name, StringComparison.Ordinal)) continue;

        var value = line[(separatorIndex + 1)..].Trim();
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') ||
             (value[0] == '\'' && value[^1] == '\''))) value = value[1..^1];

        return value;
    }

    return null;
}

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
