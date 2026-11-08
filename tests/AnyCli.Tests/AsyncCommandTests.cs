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

    private static async Task<TestAppResult> RunAsync<TCommand>(string commandName)
        where TCommand : NoParametersAsyncCommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<TCommand>(commandName))
            .Build();

        return await TestAppHarness.RunAsync(app, [commandName]);
    }

    private sealed class ReturnIntAsyncCommand : NoParametersAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            return 42;
        }
    }

    private sealed class DelayedCommand : NoParametersAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            context.Console.Output.Write("done");
            return 0;
        }
    }
}
