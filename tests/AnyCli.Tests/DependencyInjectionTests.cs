// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class DependencyInjectionTests
{
    [Test]
    public async Task Command_IsResolvedFromProvider()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new GreetCommand(new GreetingService("hello")));

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands => commands.AddCommand<GreetCommand>("greet"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["greet"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task AsyncCommand_IsResolvedFromProvider()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new AsyncGreetCommand(new GreetingService("hello")));

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands => commands.AddAsyncCommand<AsyncGreetCommand>("greet"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["greet"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("async:hello");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task Command_ResolvesNewInstancePerRun()
    {
        var created = 0;
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() =>
        {
            created++;
            return new GreetCommand(new GreetingService("hello"));
        });

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands => commands.AddCommand<GreetCommand>("greet"))
            .Build();

        await TestAppHarness.RunAsync(app, ["greet"]);
        await TestAppHarness.RunAsync(app, ["greet"]);

        await Assert.That(created).IsEqualTo(2);
    }

    [Test]
    public async Task HelpProvider_IsResolvedFromProvider()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new InjectedHelpProvider(new GreetingService("help")));

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(_ => { })
            .AddHelp<InjectedHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("help");
        await Assert.That(result.Error).IsEmpty();
    }


    private sealed class GreetingService(string greeting)
    {
        public string Greeting { get; } = greeting;
    }

    private sealed class InjectedHelpProvider(GreetingService greeting) : IHelpProvider
    {
        public void WriteHelp(HelpContext context) => context.Console.Output.Write(greeting.Greeting);
    }

    private sealed class GreetCommand(GreetingService greeting) : EmptyCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write(greeting.Greeting);
            return 0;
        }
    }

    private sealed class AsyncGreetCommand(GreetingService greeting) : EmptyAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            context.Console.Output.Write($"async:{greeting.Greeting}");
            return 0;
        }
    }
}
