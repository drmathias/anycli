// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class HelpLocalizationTests
{
    [Test]
    public async Task DefaultHelpProvider_IsUsedWithoutConfiguration()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<NoOpCommand>("run", "Runs the command"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(result.Output).Contains("Usage:");
        await Assert.That(result.Output).Contains("Options:");
        await Assert.That(result.Output).Contains("Commands:");
        await Assert.That(result.Output).Contains("run");
        await Assert.That(result.Output).Contains("--help");
    }

    [Test]
    public async Task ConfigureLocalization_UseCultureFunc_IsEvaluatedAtRuntime()
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("en");
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture(() => culture))
            .ConfigureCommands(commands => commands.AddCommand<NoOpCommand>("run"))
            .Build();

        culture = System.Globalization.CultureInfo.GetCultureInfo("fr");
        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Utilisation:");
    }

    [Test]
    public async Task DefaultHelpProvider_LocalizesSectionTitles()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("fr"))
            .ConfigureCommands(commands => commands.AddCommand<ArgumentCommand, ArgumentParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Utilisation:");
        await Assert.That(result.Output).Contains("Arguments:");
        await Assert.That(result.Output).Contains("Options:");
        await Assert.That(result.Output).DoesNotContain("Options globales:");
    }

    [Test]
    public async Task HelpDescription_UsesConfiguredResourceManager()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("fr"))
            .ConfigureHelp(help => help
                .UseLocalizedTextProvider(
                    Resources.HelpDescriptions.ResourceManager))
            .ConfigureCommands(commands => commands.AddCommand<ArgumentCommand, ArgumentParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Utilisation:");
        await Assert.That(result.Output).Contains("Description du fichier d’entrée");
    }

    [Test]
    public async Task HelpDescription_UsesLiteralFallbackWhenKeyIsMissing()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureHelp(help => help
                .UseLocalizedTextProvider(
                    Resources.HelpDescriptions.ResourceManager))
            .ConfigureCommands(commands => commands.AddCommand<FallbackCommand, FallbackParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Fallback input");
    }

    [Test]
    public async Task BuiltInHelpDescriptions_AreLocalized()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("1.0.0")
            .ConfigureLocalization(LocalizationOptions.FromCulture("de"))
            .ConfigureCommands(_ => { })
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Zeigt Hilfeinformationen an");
        await Assert.That(result.Output).Contains("Zeigt Versionsinformationen an");
    }

    [Test]
    public async Task MissingDescriptionResourceManifest_FallsBackToLiteralDescription()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureHelp(help => help
                .UseLocalizedTextProvider(
                    "AnyCli.Tests.Resources.MissingHelpDescriptions",
                    typeof(HelpLocalizationTests).Assembly))
            .ConfigureCommands(commands => commands.AddCommand<ArgumentCommand, ArgumentParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Input file");
    }

    [Test]
    public async Task HelpResources_ContainSectionTitleTranslations()
    {
        var resources = new System.Resources.ResourceManager(
            "AnyCli.Help.HelpMessages",
            typeof(LocalizedHelpTextProvider).Assembly);

        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fr"] = "Utilisation",
            ["de"] = "Verwendung",
            ["es"] = "Uso",
            ["pt-BR"] = "Uso",
            ["it"] = "Utilizzo",
            ["nl"] = "Gebruik",
        };

        foreach (var (cultureName, expectedUsage) in expected)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureName);
            var usage = resources.GetString("Usage", culture);

            await Assert.That(usage).IsEqualTo(expectedUsage);
        }
    }

    [Test]
    public async Task HelpResources_ContainDefaultValueTranslations()
    {
        var resources = new System.Resources.ResourceManager(
            "AnyCli.Help.HelpMessages",
            typeof(LocalizedHelpTextProvider).Assembly);

        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["en"] = "default",
            ["fr"] = "par défaut",
            ["de"] = "Standard",
            ["es"] = "predeterminado",
            ["pt-BR"] = "padrão",
            ["it"] = "predefinito",
            ["nl"] = "standaard",
        };

        foreach (var (cultureName, expectedDefault) in expected)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureName);
            var defaultValue = resources.GetString("DefaultValue", culture);

            await Assert.That(defaultValue).IsEqualTo(expectedDefault);
        }
    }

    [Test]
    public async Task HelpResources_ContainUsagePlaceholderTranslations()
    {
        var resources = new System.Resources.ResourceManager(
            "AnyCli.Help.HelpMessages",
            typeof(LocalizedHelpTextProvider).Assembly);

        var expected = new Dictionary<string, (string Options, string Command)>(StringComparer.Ordinal)
        {
            ["fr"] = ("OPTIONS", "COMMANDE"),
            ["de"] = ("OPTIONEN", "BEFEHL"),
            ["es"] = ("OPCIONES", "COMANDO"),
            ["pt-BR"] = ("OPÇÕES", "COMANDO"),
            ["it"] = ("OPZIONI", "COMANDO"),
            ["nl"] = ("OPTIES", "OPDRACHT"),
        };

        foreach (var (cultureName, expectedPlaceholders) in expected)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureName);
            var options = resources.GetString("OptionsPlaceholder", culture);
            var command = resources.GetString("CommandPlaceholder", culture);

            await Assert.That(options).IsEqualTo(expectedPlaceholders.Options);
            await Assert.That(command).IsEqualTo(expectedPlaceholders.Command);
        }
    }

    private sealed class NoOpCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context) => 0;
    }

    private sealed class ArgumentCommand : ICommand<ArgumentParameters>
    {
        public int Execute(CommandContext context, ArgumentParameters parameters) => 0;
    }

    private sealed class FallbackCommand : ICommand<FallbackParameters>
    {
        public int Execute(CommandContext context, FallbackParameters parameters) => 0;
    }
}

public sealed partial class ArgumentParameters
{
    [Argument(0)]
    [HelpDescription("Input file", ResourceKey = "InputDescription")]
    public string Input { get; set; } = "";
}

public sealed partial class FallbackParameters
{
    [Argument(0)]
    [HelpDescription("Fallback input", ResourceKey = "MissingDescription")]
    public string Input { get; set; } = "";
}
