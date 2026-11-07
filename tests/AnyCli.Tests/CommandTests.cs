// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using AnyCli;

namespace AnyCli.Tests;

public class CommandTests
{
    [Test]
    public async Task Command_ReturningInt_ReturnsExitCode()
    {
        var result = await Run<ReturnIntCommand>("add");

        await Assert.That(result.ExitCode).IsEqualTo(42);
    }

    [Test]
    public async Task Command_ReturningCustomEnum_ConvertsToInt()
    {
        var result = await Run<ReturnEnumCommand>("deploy");

        await Assert.That(result.ExitCode).IsEqualTo((int)DeployExitCode.InvalidConfiguration);
    }

    [Test]
    public async Task Command_ReceivesContextWithNameAndWriters()
    {
        var result = await Run<DescribeCommand>("describe");

        await Assert.That(result.Output).IsEqualTo("describe");
        await Assert.That(result.Error).IsEmpty();
        await Assert.That(result.ExitCode).IsEqualTo(0);
    }

    [Test]
    public async Task Command_ReadsFromInput()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddCommand<ReadInputCommand>("read")
            .Build();

        using var input = new StringReader("hello");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(["read"], new TextConsole(input, output, error));

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(output.ToString()).IsEqualTo("hello");
        await Assert.That(error.ToString()).IsEmpty();
    }

    [Test]
    public async Task Command_WritesToErrorWriter()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddCommand<WriteErrorCommand>("fail")
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(["fail"], output, error);

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(output.ToString()).IsEmpty();
        await Assert.That(error.ToString()).IsEqualTo("boom");
    }

    [Test]
    public async Task VersionFlag_StillWorksWhenCommandsAreRegistered()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("9.9.9")
            .AddCommand<ReturnIntCommand>("add")
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(["--version"], output, error);

        await Assert.That(output.ToString()).IsEqualTo($"9.9.9{Environment.NewLine}");
        await Assert.That(exitCode).IsEqualTo(0);
    }

    private static async Task<RunResult> Run<TCommand>(string commandName)
        where TCommand : class, ICommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddCommand<TCommand>(commandName)
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync([commandName], output, error);

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private enum DeployExitCode
    {
        Success = 0,
        InvalidConfiguration = 3,
    }

    private sealed class ReturnIntCommand : ICommand
    {
        public int Execute(CommandContext context) => 42;
    }

    private sealed class ReturnEnumCommand : ICommand<DeployExitCode>
    {
        public DeployExitCode Execute(CommandContext context) => DeployExitCode.InvalidConfiguration;
    }

    private sealed class DescribeCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Console.Output.Write(context.Name);
            return 0;
        }
    }

    private sealed class WriteErrorCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Console.Error.Write("boom");
            return 0;
        }
    }

    private sealed class ReadInputCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Console.Output.Write(context.Console.Input.ReadLine());
            return 0;
        }
    }
}
