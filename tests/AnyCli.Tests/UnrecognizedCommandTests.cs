// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class UnrecognizedCommandTests
{
    [Test]
    public async Task DefaultHandler_WritesToErrorAndReturnsDefaultExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(_ => { })
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"Unknown command 'nope'.{Environment.NewLine}");
    }

    [Test]
    public async Task DefaultHandler_ReportsMissingCommand()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(_ => { })
            .Build();

        var result = await TestAppHarness.RunAsync(app, []);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"No command was specified.{Environment.NewLine}");
    }

    [Test]
    public async Task CustomHandler_CanWriteToOutputAndUseCustomExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(_ => { })
            .UseUnrecognizedCommandHandler<OutputUnrecognizedCommandHandler>(7)
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(7);
        await Assert.That(result.Output).IsEqualTo($"custom:nope{Environment.NewLine}");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task GenericRegistrar_AcceptsEnumExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands<UnrecognizedExitCode>(_ => { })
            .WithName("anycli")
            .UseUnrecognizedCommandHandler<DefaultUnrecognizedCommandHandler>(UnrecognizedExitCode.UnknownCommand)
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)UnrecognizedExitCode.UnknownCommand);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("nope");
    }

    [Test]
    public async Task Handler_IsResolvedFromServiceProvider()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new InjectedUnrecognizedCommandHandler("injected"));

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(_ => { })
            .UseUnrecognizedCommandHandler<InjectedUnrecognizedCommandHandler>(3)
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(3);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"injected{Environment.NewLine}");
    }


    private sealed class OutputUnrecognizedCommandHandler : IUnrecognizedCommandHandler
    {
        public ValueTask HandleAsync(
            UnrecognizedCommandContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.WriteLine($"custom:{context.AttemptedCommandPath}");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class InjectedUnrecognizedCommandHandler(string message) : IUnrecognizedCommandHandler
    {
        public ValueTask HandleAsync(
            UnrecognizedCommandContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Error.WriteLine(message);
            return ValueTask.CompletedTask;
        }
    }
}

public enum UnrecognizedExitCode
{
    UnknownCommand = 9,
}
