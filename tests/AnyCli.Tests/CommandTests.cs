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
            context.Output.Write(context.Name);
            return 0;
        }
    }
}
