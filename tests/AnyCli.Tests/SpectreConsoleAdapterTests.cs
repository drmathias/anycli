// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.SpectreConsole;

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

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            ["--help"],
            new TextConsole(new StringReader(string.Empty), output, error));

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(output.ToString().Contains("help", StringComparison.Ordinal)).IsTrue();
        await Assert.That(error.ToString()).IsEmpty();
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

    private static async Task<RunResult> Run<TCommand>(string commandName)
        where TCommand : EmptyCommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand>(commandName))
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            [commandName],
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class PropertyCommand : EmptyCommand
    {
        public override int Execute(CommandContext context)
        {
            context.AnsiConsole.Output.Markup("out");
            context.AnsiConsole.Error.Markup("err");
            return 0;
        }
    }

    private sealed class CachingCommand : EmptyCommand
    {
        public override int Execute(CommandContext context)
        {
            var first = context.AnsiConsole;
            var second = context.AnsiConsole;
            context.Console.Output.Write(ReferenceEquals(first, second) ? "same" : "different");
            return 0;
        }
    }

    private sealed class MixedCommand : EmptyCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.WriteLine("raw");
            context.AnsiConsole.Output.Markup("ok");
            return 0;
        }
    }

    private sealed class ErrorCommand : EmptyCommand
    {
        public override int Execute(CommandContext context)
        {
            context.AnsiConsole.Error.Markup("boom");
            return 0;
        }
    }

    private sealed class AnsiHelpProvider : IHelpProvider
    {
        public void WriteHelp(HelpContext context)
        {
            context.AnsiConsole.Output.Markup("help");
        }
    }
}
