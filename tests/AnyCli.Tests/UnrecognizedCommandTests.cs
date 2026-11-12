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
    public async Task CustomHandler_WritesToErrorAndUsesCustomExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(_ => { })
            .UseErrorHandler<OutputErrorHandler>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(7);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"custom:nope{Environment.NewLine}");
    }

    [Test]
    public async Task GenericRegistrar_AcceptsEnumExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands<UnrecognizedExitCode>(_ => { })
            .WithName("anycli")
            .UseErrorHandler<EnumErrorHandler>()
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
        serviceProvider.Add(() => new InjectedErrorHandler("injected"));

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(_ => { })
            .UseErrorHandler<InjectedErrorHandler>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(3);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"injected{Environment.NewLine}");
    }

    private abstract class TestErrorHandler : ICommandErrorHandler
    {
        public abstract ValueTask<int> HandleAsync(
            CommandErrorContext context,
            CancellationToken cancellationToken);
    }

    private sealed class OutputErrorHandler : TestErrorHandler
    {
        public override ValueTask<int> HandleAsync(
            CommandErrorContext context,
            CancellationToken cancellationToken)
        {
            var path = context.Error is UnknownCommandError unknown
                ? unknown.AttemptedCommandPath
                : context.CommandPath.Count == 0
                    ? string.Empty
                    : string.Join(' ', context.CommandPath);

            context.Console.Error.WriteLine($"custom:{path}");
            return ValueTask.FromResult(7);
        }
    }

    private sealed class EnumErrorHandler : ICommandErrorHandler<UnrecognizedExitCode>
    {
        public ValueTask<UnrecognizedExitCode> HandleAsync(
            CommandErrorContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Error.WriteLine("nope");
            return ValueTask.FromResult(UnrecognizedExitCode.UnknownCommand);
        }
    }

    private sealed class InjectedErrorHandler(string message) : ICommandErrorHandler
    {
        public ValueTask<int> HandleAsync(
            CommandErrorContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Error.WriteLine(message);
            return ValueTask.FromResult(3);
        }
    }
}

public enum UnrecognizedExitCode
{
    UnknownCommand = 9,
}
