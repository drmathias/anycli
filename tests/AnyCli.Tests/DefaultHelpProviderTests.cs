// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class DefaultHelpProviderTests
{
    private const string ExpectedRootHelp =
        """
        Usage:
            anycli [OPTIONS] <COMMAND>

        Options:
            -h, --help     Prints help information
            -v, --version  Prints version information

        Commands:
            run     Run the command
            remote  Manage remotes
        """;

    private const string ExpectedRunHelp =
        """
        Description:
            Run the command

        Usage:
            anycli run <input> [OPTIONS]

        Arguments:
            <input>  Input file

        Options:
            --count        Number of items [default: three items]
            --mode         Processing mode [default: text]
            --verbose      Enable verbose output
            -h, --help     Prints help information
            -v, --version  Prints version information
        """;

    private const string ExpectedRemoteHelp =
        """
        Description:
            Manage remotes

        Usage:
            anycli remote [OPTIONS] <COMMAND>

        Options:
            -h, --help     Prints help information
            -v, --version  Prints version information

        Commands:
            add  Add a remote
        """;

    private const string ExpectedVariadicHelp =
        """
        Description:
            Copy files

        Usage:
            anycli copy <source> [destinations...] [OPTIONS]

        Arguments:
            <source>           Source
            [destinations...]  Destinations

        Options:
            -h, --help     Prints help information
            -v, --version  Prints version information
        """;

    private const string ExpectedSpanishRootHelp =
        """
        Uso:
            anycli [OPCIONES] <COMANDO>

        Opciones:
            -h, --help     Muestra la información de ayuda
            -v, --version  Muestra la información de versión

        Comandos:
            run     Ejecuta el comando
            remote  Gestiona los remotos
        """;

    private const string ExpectedSpanishRunHelp =
        """
        Descripción:
            Ejecuta el comando

        Uso:
            anycli run <input> [OPCIONES]

        Argumentos:
            <input>  Archivo de entrada

        Opciones:
            --count        Número de elementos [predeterminado: tres elementos]
            --mode         Modo de procesamiento [predeterminado: text]
            --verbose      Habilitar salida detallada
            -h, --help     Muestra la información de ayuda
            -v, --version  Muestra la información de versión
        """;

    [Test]
    public async Task RootHelp_DefaultProvider_RendersFullOutput()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedRootHelp));
    }

    [Test]
    public async Task CommandHelp_DefaultProvider_RendersFullOutput()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedRunHelp));
    }

    [Test]
    public async Task CommandHelp_DefaultProvider_RendersDefaultValues()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(result.Output).Contains("[default: three items]");
        await Assert.That(result.Output).Contains("[default: text]");
    }

    [Test]
    public async Task VariadicArgumentHelp_DefaultProvider_RendersFullOutput()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("1.0.0")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureCommands(commands =>
                commands.AddCommand<VariadicCopyCommand, VariadicCopyParameters>("copy", "Copy files"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["copy", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedVariadicHelp));
    }

    [Test]
    public async Task RootHelp_DefaultProvider_RendersSpanishOutput()
    {
        var app = CreateApp("es");

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedSpanishRootHelp));
    }

    [Test]
    public async Task CommandHelp_DefaultProvider_RendersSpanishOutput()
    {
        var app = CreateApp("es");

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedSpanishRunHelp));
    }

    [Test]
    public async Task GroupHelp_DefaultProvider_RendersFullOutput()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["remote", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedRemoteHelp));
    }

    private static ConsoleApp CreateApp(string culture = "en") =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("1.0.0")
            .ConfigureLocalization(LocalizationOptions.FromCulture(culture))
            .ConfigureHelp(help => help
                .UseLocalizedTextProvider(
                    Resources.HelpDescriptions.ResourceManager))
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<RunCommand, RunParameters>(
                    "run",
                    HelpText.FromResource("RunDescription"));
                commands.AddGroup(
                    "remote",
                    HelpText.FromResource("ManageRemotesDescription"),
                    group => group.AddCommand<NoOpCommand>("add", "Add a remote"));
            })
            .Build();

    private static string Normalize(string value) =>
        string.Join(
            '\n',
            value.ReplaceLineEndings("\n")
                .Split('\n')
                .Select(line => line.TrimEnd()))
            .TrimEnd('\n');

    private sealed class RunCommand : ICommand<RunParameters>
    {
        public int Execute(CommandContext context, RunParameters parameters) => 0;
    }

    private sealed class VariadicCopyCommand : ICommand<VariadicCopyParameters>
    {
        public int Execute(CommandContext context, VariadicCopyParameters parameters) => 0;
    }

    private sealed class NoOpCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context) => 0;
    }
}

public sealed partial class RunParameters
{
    [Argument(0)]
    [HelpDescription(ResourceKey = "InputDescription")]
    public string Input { get; set; } = "";

    [Option("--count")]
    [HelpDescription(ResourceKey = "CountDescription")]
    [HelpDefaultValue(ResourceKey = "CountDefault")]
    public int Count { get; set; } = 3;

    [Option("--mode")]
    [HelpDescription(ResourceKey = "ModeDescription")]
    public OutputMode Mode { get; set; } = OutputMode.Text;

    [Option("--verbose")]
    [HelpDescription(ResourceKey = "VerboseDescription")]
    public bool Verbose { get; set; }
}

public enum OutputMode
{
    Text,
    Json,
}

public sealed partial class VariadicCopyParameters
{
    [Argument(0)]
    [HelpDescription("Source")]
    public string Source { get; set; } = "";

    [VariadicArgument(1)]
    [HelpDescription("Destinations")]
    public string[] Destinations { get; set; } = [];
}
