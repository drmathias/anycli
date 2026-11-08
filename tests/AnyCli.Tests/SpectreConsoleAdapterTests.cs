// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.SpectreConsole;
using AnyCli.Tests.Fixtures;

using Spectre.Console;

namespace AnyCli.Tests;

public class SpectreConsoleAdapterTests
{
    [Test]
    public async Task Command_AnsiConsolePropertyWritesToOutputAndError()
    {
        var result = await Run<PropertyCommand>("property");

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output.Contains("out", StringComparison.Ordinal)).IsTrue();
        await Assert.That(result.Error.Contains("err", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task Command_AnsiConsolePropertyIsCached()
    {
        var result = await Run<CachingCommand>("caching");

        await Assert.That(result.Output).IsEqualTo("same");
    }

    [Test]
    public async Task Command_CanMixRawAndAnsiOutput()
    {
        var result = await Run<MixedCommand>("mixed");

        await Assert.That(result.Output.StartsWith($"raw{Environment.NewLine}", StringComparison.Ordinal)).IsTrue();
        await Assert.That(result.Output.Contains("ok", StringComparison.Ordinal)).IsTrue();
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task Command_WritesToAnsiErrorConsole()
    {
        var result = await Run<ErrorCommand>("error");

        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error.Contains("boom", StringComparison.Ordinal)).IsTrue();
    }

    [Test]
    public async Task HelpProvider_WritesToAnsiConsole()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddHelp<AnsiHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output.Contains("help", StringComparison.Ordinal)).IsTrue();
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task UnrecognizedCommand_UsesSpectreErrorHandler()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(_ => { })
            .UseUnrecognizedCommandHandler<DefaultSpectreUnrecognizedCommandHandler>(4)
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(4);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("Unknown command");
        await Assert.That(result.Error).Contains("nope");
    }

    [Test]
    public async Task Create_CanForceAnsiForTesting()
    {
        using var output = new StringWriter();
        var console = SpectreConsoleAdapter.Create(
            new TextConsole(new StringReader(string.Empty), output, new StringWriter()).Output,
            SpectreConsoleOptions.ForTesting);

        console.Markup("[green]ok[/]");

        await Assert.That(output.ToString()).IsEqualTo("\u001b[32mok\u001b[0m");
    }

    private static async Task<TestAppResult> Run<TCommand>(string commandName)
        where TCommand : NoParametersCommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand>(commandName))
            .Build();

        return await TestAppHarness.RunAsync(app, [commandName]);
    }

    private sealed class PropertyCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.AnsiConsole.Output.Markup("out");
            context.AnsiConsole.Error.Markup("err");
            return 0;
        }
    }

    private sealed class CachingCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            var first = context.AnsiConsole;
            var second = context.AnsiConsole;
            context.Console.Output.Write(ReferenceEquals(first, second) ? "same" : "different");
            return 0;
        }
    }

    private sealed class MixedCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.WriteLine("raw");
            context.AnsiConsole.Output.Markup("ok");
            return 0;
        }
    }

    private sealed class ErrorCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.AnsiConsole.Error.Markup("boom");
            return 0;
        }
    }

    private sealed class AnsiHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context) =>
            context.AnsiConsole.Output.Markup("help");

        public void WriteCommandHelp(CommandHelpContext context) =>
            context.AnsiConsole.Output.Markup("help");
    }
}
