// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

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
            .ConfigureCommands(commands => commands.AddCommand<ReadInputCommand>("read"))
            .Build();

        using var input = new StringReader("hello");

        var result = await TestAppHarness.RunAsync(app, ["read"], input);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task Command_WritesToErrorWriter()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<WriteErrorCommand>("fail"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["fail"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo("boom");
    }

    [Test]
    public async Task VersionFlag_StillWorksWhenCommandsAreRegistered()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("9.9.9")
            .ConfigureCommands(commands => commands.AddCommand<ReturnIntCommand>("add"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--version"]);

        await Assert.That(result.Output).IsEqualTo($"9.9.9{Environment.NewLine}");
        await Assert.That(result.ExitCode).IsEqualTo(0);
    }

    private static async Task<TestAppResult> Run<TCommand>(string commandName)
        where TCommand : NoParametersCommand, new()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand>(commandName))
            .Build();

        return await TestAppHarness.RunAsync(app, [commandName]);
    }

    private sealed class ReturnIntCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context) => 42;
    }

    private sealed class DescribeCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write(context.Name);
            return 0;
        }
    }

    private sealed class WriteErrorCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Error.Write("boom");
            return 0;
        }
    }

    private sealed class ReadInputCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write(context.Console.Input.ReadLine());
            return 0;
        }
    }
}
