using AnyCli;

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
    public async Task ConfigureCommandsOfT_RegistersEnumCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands<TestExitCode>(commands => commands
                .AddCommand<EnumCommand>("enum"))
            .Build();

        var result = await Run(app, ["enum"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)TestExitCode.Failed);
    }

    [Test]
    public async Task ConfigureCommandsOfT_RegistersAsyncEnumCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands<TestExitCode>(commands => commands
                .AddAsyncCommand<AsyncEnumCommand>("async"))
            .Build();

        var result = await Run(app, ["async"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)TestExitCode.Failed);
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

    private sealed class IntCommand : ICommand
    {
        public int Execute(CommandContext context) => 7;
    }

    private sealed class EnumCommand : IExitCodeCommand<TestExitCode>
    {
        public TestExitCode Execute(CommandContext context) => TestExitCode.Failed;
    }

    private sealed class AsyncEnumCommand : IAsyncExitCodeCommand<TestExitCode>
    {
        public async Task<TestExitCode> ExecuteAsync(CommandContext context)
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
