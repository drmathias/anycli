// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class ConfigureCommandsTests
{
    [Test]
    public async Task ConfigureCommands_RegistersIntegerCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<IntCommand>("int"))
            .Build();

        var result = await Run(app, ["int"]);

        await Assert.That(result.ExitCode).IsEqualTo(7);
    }

    [Test]
    public async Task ConfigureCommands_RegistersAsyncCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<AsyncIntCommand>("async"))
            .Build();

        var result = await Run(app, ["async"]);

        await Assert.That(result.ExitCode).IsEqualTo(8);
    }

    [Test]
    public async Task ConfigureCommandsOfT_RegistersEmptyEnumCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands<TestExitCode>(commands => commands
                .AddCommand<EnumCommand>("enum")
                .AddAsyncCommand<AsyncEnumCommand>("async"))
            .Build();

        var syncResult = await Run(app, ["enum"]);
        var asyncResult = await Run(app, ["async"]);

        await Assert.That(syncResult.ExitCode).IsEqualTo((int)TestExitCode.Failed);
        await Assert.That(asyncResult.ExitCode).IsEqualTo((int)TestExitCode.Failed);
    }

    private static async Task<RunResult> Run(ConsoleApp app, string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class IntCommand : EmptyCommand
    {
        public override int Execute(CommandContext context) => 7;
    }

    private sealed class AsyncIntCommand : EmptyAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            return 8;
        }
    }

    private sealed class EnumCommand : EmptyCommand<TestExitCode>
    {
        public override TestExitCode Execute(CommandContext context) => TestExitCode.Failed;
    }

    private sealed class AsyncEnumCommand : EmptyAsyncCommand<TestExitCode>
    {
        public override async Task<TestExitCode> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            return TestExitCode.Failed;
        }
    }

    private enum TestExitCode
    {
        Success = 0,
        Failed = 3,
    }
}
