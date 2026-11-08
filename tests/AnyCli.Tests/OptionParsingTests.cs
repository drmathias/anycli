// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class OptionParsingTests
{
    [Test]
    public async Task ParsesLongOptionValues()
    {
        var result = await Run<OptionParametersCommand, OptionParameters>(
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
        var result = await Run<OptionParametersCommand, OptionParameters>(
            "opts",
            ["opts", "--config=cfg", "-o=out", "--tag=1", "--tag=2", "-vq", "--count=4", "--dry-run"]);

        await Assert.That(result.Output).IsEqualTo("cfg|out|True|True|1,2|4|True");
    }

    [Test]
    public async Task ParsesShortValueOptionWithSeparateValue()
    {
        var result = await Run<OptionParametersCommand, OptionParameters>(
            "opts",
            ["opts", "--config", "cfg", "-o", "short"]);

        await Assert.That(result.Output).IsEqualTo("cfg|short|False|False||0|False");
    }

    [Test]
    public async Task ParsesExplicitBooleanValue()
    {
        var result = await Run<OptionParametersCommand, OptionParameters>(
            "opts",
            ["opts", "--config", "cfg", "--verbose=false"]);

        await Assert.That(result.Output).IsEqualTo("cfg|default|False|False||0|False");
    }

    [Test]
    public async Task ScalarOption_LastValueWins()
    {
        var result = await Run<OptionParametersCommand, OptionParameters>(
            "opts",
            ["opts", "--config", "cfg", "--count", "1", "--count", "2"]);

        await Assert.That(result.Output).IsEqualTo("cfg|default|False|False||2|False");
    }

    [Test]
    public async Task RequiredOption_IsEnforced()
    {
        CommandParameterException? exception = null;

        try
        {
            _ = await Run<OptionParametersCommand, OptionParameters>("opts", ["opts", "--output", "out"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'--config'");
    }

    [Test]
    public async Task UnknownOption_Throws()
    {
        CommandParameterException? exception = null;

        try
        {
            _ = await Run<OptionParametersCommand, OptionParameters>(
                "opts",
                ["opts", "--config", "cfg", "--nope"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'--nope'");
    }

    [Test]
    public async Task AttachedShortValue_IsRejected()
    {
        CommandParameterException? exception = null;

        try
        {
            _ = await Run<OptionParametersCommand, OptionParameters>(
                "opts",
                ["opts", "--config", "cfg", "-oout"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("separate value");
    }

    [Test]
    public async Task OptionsCanBeInterspersedWithArguments()
    {
        var result = await Run<OptionAndArgumentCommand, OptionAndArgumentParameters>(
            "mix",
            ["mix", "--count", "3", "name"]);

        await Assert.That(result.Output).IsEqualTo("name|3");
    }

    [Test]
    public async Task DoubleDashStopsOptionParsing()
    {
        var result = await Run<OptionAndArgumentCommand, OptionAndArgumentParameters>(
            "mix",
            ["mix", "--", "--count"]);

        await Assert.That(result.Output).IsEqualTo("--count|0");
    }

    private static async Task<RunResult> Run<TCommand, TParameters>(
        string commandName,
        string[] args)
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, ICommandParameters<TParameters>
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TParameters>(commandName))
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class OptionParametersCommand : ICommand<OptionParameters>
    {
        public int Execute(CommandContext context, OptionParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Config}|{parameters.Output}|{parameters.Verbose}|{parameters.Quiet}|" +
                $"{string.Join(",", parameters.Tags)}|{parameters.Count}|{parameters.DryRun}");
            return 0;
        }
    }

    private sealed class OptionAndArgumentCommand : ICommand<OptionAndArgumentParameters>
    {
        public int Execute(CommandContext context, OptionAndArgumentParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Name}|{parameters.Count}");
            return 0;
        }
    }
}

public sealed partial class OptionParameters
{
    [RequiredOption("--config")]
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

public sealed partial class OptionAndArgumentParameters
{
    [Argument(0)]
    public string Name { get; set; } = "";

    [Option("--count")]
    public int Count { get; set; }
}
