// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class OptionParsingTests
{
    [Test]
    public async Task ParsesLongOptionValues()
    {
        var result = await Run<OptionSettingsCommand, OptionSettings>(
            "opts",
            [
                "opts",
                "--config", "cfg",
                "--output", "out",
                "--tag", "1",
                "--tag", "2",
                "--verbose",
                "--quiet",
                "--count", "3",
                "--dry-run",
            ]);

        await Assert.That(result.Output).IsEqualTo("cfg|out|True|True|1,2|3|True");
    }

    [Test]
    public async Task ParsesEqualsAndShortAliases()
    {
        var result = await Run<OptionSettingsCommand, OptionSettings>(
            "opts",
            ["opts", "--config=cfg", "-o=out", "--tag=1", "--tag=2", "-vq", "--count=4", "--dry-run"]);

        await Assert.That(result.Output).IsEqualTo("cfg|out|True|True|1,2|4|True");
    }

    [Test]
    public async Task ParsesShortValueOptionWithSeparateValue()
    {
        var result = await Run<OptionSettingsCommand, OptionSettings>(
            "opts",
            ["opts", "--config", "cfg", "-o", "short"]);

        await Assert.That(result.Output).IsEqualTo("cfg|short|False|False||0|False");
    }

    [Test]
    public async Task ParsesExplicitBooleanValue()
    {
        var result = await Run<OptionSettingsCommand, OptionSettings>(
            "opts",
            ["opts", "--config", "cfg", "--verbose=false"]);

        await Assert.That(result.Output).IsEqualTo("cfg|default|False|False||0|False");
    }

    [Test]
    public async Task ScalarOption_LastValueWins()
    {
        var result = await Run<OptionSettingsCommand, OptionSettings>(
            "opts",
            ["opts", "--config", "cfg", "--count", "1", "--count", "2"]);

        await Assert.That(result.Output).IsEqualTo("cfg|default|False|False||2|False");
    }

    [Test]
    public async Task RequiredOption_IsEnforced()
    {
        CommandArgumentException? exception = null;

        try
        {
            _ = await Run<OptionSettingsCommand, OptionSettings>("opts", ["opts", "--output", "out"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'--config'");
    }

    [Test]
    public async Task UnknownOption_Throws()
    {
        CommandArgumentException? exception = null;

        try
        {
            _ = await Run<OptionSettingsCommand, OptionSettings>(
                "opts",
                ["opts", "--config", "cfg", "--nope"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'--nope'");
    }

    [Test]
    public async Task AttachedShortValue_IsRejected()
    {
        CommandArgumentException? exception = null;

        try
        {
            _ = await Run<OptionSettingsCommand, OptionSettings>(
                "opts",
                ["opts", "--config", "cfg", "-oout"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("separate value");
    }

    [Test]
    public async Task OptionsCanBeInterspersedWithArguments()
    {
        var result = await Run<OptionAndArgumentCommand, OptionAndArgumentSettings>(
            "mix",
            ["mix", "--count", "3", "name"]);

        await Assert.That(result.Output).IsEqualTo("name|3");
    }

    [Test]
    public async Task DoubleDashStopsOptionParsing()
    {
        var result = await Run<OptionAndArgumentCommand, OptionAndArgumentSettings>(
            "mix",
            ["mix", "--", "--count"]);

        await Assert.That(result.Output).IsEqualTo("--count|0");
    }

    private static async Task<RunResult> Run<TCommand, TSettings>(
        string commandName,
        string[] args)
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TSettings>(commandName))
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class OptionSettingsCommand : ICommand<OptionSettings>
    {
        public int Execute(CommandContext context, OptionSettings settings)
        {
            context.Console.Output.Write(
                $"{settings.Config}|{settings.Output}|{settings.Verbose}|{settings.Quiet}|" +
                $"{string.Join(",", settings.Tags)}|{settings.Count}|{settings.DryRun}");
            return 0;
        }
    }

    private sealed class OptionAndArgumentCommand : ICommand<OptionAndArgumentSettings>
    {
        public int Execute(CommandContext context, OptionAndArgumentSettings settings)
        {
            context.Console.Output.Write($"{settings.Name}|{settings.Count}");
            return 0;
        }
    }
}

public sealed partial class OptionSettings
{
    [Option("--config", Required = true)]
    public string Config { get; set; } = "";

    [Option("--output", "-o")]
    public string Output { get; set; } = "default";

    [Option("--verbose", "-v")]
    public bool Verbose { get; set; }

    [Option("--quiet", "-q")]
    public bool Quiet { get; set; }

    [Option("--tag")]
    public List<int> Tags { get; set; } = [];

    [Option("--count")]
    public int Count { get; set; }

    [Option]
    public bool DryRun { get; set; }
}

public sealed partial class OptionAndArgumentSettings
{
    [Argument(0)]
    public string Name { get; set; } = "";

    [Option("--count")]
    public int Count { get; set; }
}
