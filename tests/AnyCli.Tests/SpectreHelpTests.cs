// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Text.RegularExpressions;

using AnyCli.SpectreConsole;
using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class SpectreHelpTests
{
    [Test]
    public async Task RootHelp_MatchesDefaultProvider()
    {
        var plain = await TestAppHarness.RunAsync(CreateApp(useSpectre: false), ["--help"]);
        var spectre = await TestAppHarness.RunAsync(CreateApp(useSpectre: true), ["--help"]);

        await Assert.That(spectre.ExitCode).IsEqualTo(0);
        await Assert.That(spectre.Error).IsEmpty();
        await Assert.That(Normalize(spectre.Output)).IsEqualTo(Normalize(plain.Output));
    }

    [Test]
    public async Task CommandHelp_MatchesDefaultProvider()
    {
        var plain = await TestAppHarness.RunAsync(CreateApp(useSpectre: false), ["run", "--help"]);
        var spectre = await TestAppHarness.RunAsync(CreateApp(useSpectre: true), ["run", "--help"]);

        await Assert.That(spectre.ExitCode).IsEqualTo(0);
        await Assert.That(spectre.Error).IsEmpty();
        await Assert.That(Normalize(spectre.Output)).IsEqualTo(Normalize(plain.Output));
    }

    [Test]
    public async Task LocalizedHelp_MatchesDefaultProvider()
    {
        var plainRoot = await TestAppHarness.RunAsync(
            CreateApp(useSpectre: false, culture: "es"),
            ["--help"]);
        var spectreRoot = await TestAppHarness.RunAsync(
            CreateApp(useSpectre: true, culture: "es"),
            ["--help"]);
        var plainCommand = await TestAppHarness.RunAsync(
            CreateApp(useSpectre: false, culture: "es"),
            ["run", "--help"]);
        var spectreCommand = await TestAppHarness.RunAsync(
            CreateApp(useSpectre: true, culture: "es"),
            ["run", "--help"]);

        await Assert.That(Normalize(spectreRoot.Output)).IsEqualTo(Normalize(plainRoot.Output));
        await Assert.That(Normalize(spectreCommand.Output)).IsEqualTo(Normalize(plainCommand.Output));
    }

    [Test]
    public async Task DefaultValues_MatchDefaultProvider()
    {
        var plain = await TestAppHarness.RunAsync(
            CreateDefaultValueApp(useSpectre: false),
            ["defaults", "--help"]);
        var spectre = await TestAppHarness.RunAsync(
            CreateDefaultValueApp(useSpectre: true),
            ["defaults", "--help"]);

        await Assert.That(spectre.ExitCode).IsEqualTo(0);
        await Assert.That(spectre.Error).IsEmpty();
        await Assert.That(spectre.Output).Contains("[default: tasks.json]");
        await Assert.That(spectre.Output).Contains("[default: 7]");
        await Assert.That(spectre.Output).Contains("[default: text]");
        await Assert.That(Normalize(spectre.Output)).IsEqualTo(Normalize(plain.Output));
    }

    [Test]
    public async Task LocalizedTextProvider_IsUsedBySpectreHelp()
    {
        var app = CreateApp(
            useSpectre: true,
            culture: "fr",
            useDescriptionResources: true);

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Utilisation:");
        await Assert.That(result.Output).Contains("Description du fichier d’entrée");
    }

    private static ConsoleApp CreateApp(
        bool useSpectre,
        string culture = "en",
        bool useDescriptionResources = false) =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("1.0.0")
            .ConfigureLocalization(LocalizationOptions.FromCulture(culture))
            .ConfigureHelp(help =>
            {
                if (useDescriptionResources)
                {
                    help.UseLocalizedTextProvider(
                        Resources.HelpDescriptions.ResourceManager);
                }

                if (useSpectre) help.UseSpectreHelp();
            })
            .ConfigureCommands(commands =>
                commands.AddCommand<SpectreHelpCommand, SpectreHelpParameters>(
                    "run",
                    "Run the command"))
            .Build();

    private static ConsoleApp CreateDefaultValueApp(bool useSpectre) =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureHelp(help =>
            {
                if (useSpectre) help.UseSpectreHelp();
            })
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultValueCommand, SpectreDefaultParameters>("defaults"))
            .Build();

    private static string Normalize(string value) =>
        string.Join(
            '\n',
            Regex.Replace(value, @"\x1B\[[0-9;]*m", string.Empty)
                .ReplaceLineEndings("\n")
                .Split('\n')
                .Select(line => line.TrimEnd()))
            .TrimEnd('\n');

    private sealed class DefaultValueCommand : ICommand<SpectreDefaultParameters>
    {
        public int Execute(CommandContext context, SpectreDefaultParameters parameters) => 0;
    }

    private sealed class SpectreHelpCommand : ICommand<SpectreHelpParameters>
    {
        public int Execute(CommandContext context, SpectreHelpParameters parameters) => 0;
    }
}

public sealed partial class SpectreHelpParameters
{
    [Argument(0)]
    [HelpDescription("Input file", ResourceKey = "InputDescription")]
    public string Input { get; set; } = "";

    [Option("--verbose")]
    [HelpDescription("Verbose output")]
    public bool Verbose { get; set; }
}


public sealed partial class SpectreDefaultParameters
{
    [Option("--data")]
    [HelpDefaultValue("tasks.json")]
    public string Data { get; set; } = CreateComputedDefault();

    [OptionalArgument(0)]
    public int Count { get; set; } = 7;

    [Option("--mode")]
    public SpectreOutputMode Mode { get; set; } = SpectreOutputMode.Text;

    private static string CreateComputedDefault() => "computed";
}

public enum SpectreOutputMode
{
    Text,
    Json,
}
