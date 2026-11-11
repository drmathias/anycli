// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

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
    public async Task AsyncCommand_IsAwaitedBeforeRunCompletes()
    {
        var result = await RunAsync<DelayedCommand>("slow");

        await Assert.That(result.Output).IsEqualTo("done");
    }

    [Test]
    public async Task AsyncCommand_ReceivesCancellationToken()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var result = await RunAsync<TokenAwareCommand>("token", cancellationTokenSource.Token);

        await Assert.That(result.Output).IsEqualTo("token");
    }

    [Test]
    public async Task AsyncCommand_CanReadInputWithCancellationToken()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddAsyncCommand<ReadInputAsyncCommand>("read"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["read"],
            new StringReader("hello"));

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello");
    }

    [Test]
    public async Task RunAsync_CanceledToken_PreventsCommandExecution()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        OperationCanceledException? exception = null;

        try
        {
            _ = await RunAsync<DelayedCommand>("slow", cancellationTokenSource.Token);
        }
        catch (OperationCanceledException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }

    private static Task<TestAppResult> RunAsync<TCommand>(string commandName)
        where TCommand : NoParametersAsyncCommand, new() =>
        RunAsync<TCommand>(commandName, CancellationToken.None);

    private static async Task<TestAppResult> RunAsync<TCommand>(
        string commandName,
        CancellationToken cancellationToken)
        where TCommand : NoParametersAsyncCommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<TCommand>(commandName))
            .Build();

        return await TestAppHarness.RunAsync(
            app,
            [commandName],
            new StringReader(string.Empty),
            cancellationToken);
    }

    private sealed class ReturnIntAsyncCommand : NoParametersAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            return 42;
        }
    }

    private sealed class DelayedCommand : NoParametersAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            context.Console.Output.Write("done");
            return 0;
        }
    }

    private sealed class ReadInputAsyncCommand : NoParametersAsyncCommand
    {
        public override async Task<int> ExecuteAsync(
            CommandContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write(
                await context.Console.Input.ReadLineAsync(cancellationToken));
            return 0;
        }
    }

    private sealed class TokenAwareCommand : NoParametersAsyncCommand
    {
        public override Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            context.Console.Output.Write(cancellationToken.CanBeCanceled ? "token" : "none");
            return Task.FromResult(0);
        }
    }
}
