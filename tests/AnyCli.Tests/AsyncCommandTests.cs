// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class AsyncCommandTests
{
    [Test]
    public async Task AsyncCommand_ReturningTaskOfInt_ReturnsExitCode()
    {
        var result = await RunAsync<ReturnIntAsyncCommand>("add");

        await Assert.That(result.ExitCode).IsEqualTo(42);
    }

    [Test]
    public async Task AsyncCommand_ReturningTaskOfCustomEnum_ConvertsToInt()
    {
        var result = await RunAsync<ReturnEnumAsyncCommand>("deploy");

        await Assert.That(result.ExitCode).IsEqualTo((int)DeployExitCode.InvalidConfiguration);
    }

    [Test]
    public async Task AsyncCommand_IsAwaitedBeforeRunCompletes()
    {
        var result = await RunAsync<DelayedCommand>("slow");

        await Assert.That(result.Output).IsEqualTo("done");
    }

    private static async Task<RunResult> RunAsync<TCommand>(string commandName)
        where TCommand : class, IAsyncCommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<TCommand>(commandName))
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

    private sealed class ReturnIntAsyncCommand : IAsyncCommand
    {
        public async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            return 42;
        }
    }

    private sealed class ReturnEnumAsyncCommand : IAsyncExitCodeCommand<DeployExitCode>
    {
        public async Task<DeployExitCode> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            return DeployExitCode.InvalidConfiguration;
        }
    }

    private sealed class DelayedCommand : IAsyncCommand
    {
        public async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            context.Console.Output.Write("done");
            return 0;
        }
    }
}
