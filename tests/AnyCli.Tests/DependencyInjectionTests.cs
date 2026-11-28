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
    public async Task HelpRenderer_IsResolvedFromProvider()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new InjectedHelpRenderer(new GreetingService("help")));

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(_ => { })
            .ConfigureHelp(help => help.UseRenderer<InjectedHelpRenderer>())
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("help");
        await Assert.That(result.Error).IsEmpty();
    }


    [Test]
    public async Task Command_FallsBackToParameterlessConstructor()
    {
        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(new StubServiceProvider())
            .ConfigureCommands(commands => commands.AddCommand<PlainCommand>("plain"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["plain"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("plain");
    }

    [Test]
    public async Task Interceptor_FallsBackToParameterlessConstructor()
    {
        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(new StubServiceProvider())
            .AddInterceptor<PlainInterceptor>()
            .ConfigureCommands(commands => commands.AddCommand<PlainCommand>("plain"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["plain"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("interceptor>plain");
    }

    [Test]
    public async Task HelpRenderer_FallsBackToParameterlessConstructor()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(new StubServiceProvider())
            .ConfigureCommands(_ => { })
            .ConfigureHelp(help => help.UseRenderer<PlainHelpRenderer>())
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("plain-help");
    }

    private sealed class PlainCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write("plain");
            return 0;
        }
    }

    private sealed class PlainInterceptor : ICommandInterceptor
    {
        public Task<int> InterceptAsync(
            CommandInterceptorContext context,
            Func<CancellationToken, Task<int>> next,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write("interceptor>");
            return next.Invoke(cancellationToken);
        }
    }

    private sealed class PlainHelpRenderer : IHelpRenderer
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write("plain-help");
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write("plain-help");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class GreetingService(string greeting)
    {
        public string Greeting { get; } = greeting;
    }

    private sealed class InjectedHelpRenderer(GreetingService greeting) : IHelpRenderer
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write(greeting.Greeting);
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write(greeting.Greeting);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class GreetCommand(GreetingService greeting) : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write(greeting.Greeting);
            return 0;
        }
    }

    private sealed class AsyncGreetCommand(GreetingService greeting) : NoParametersAsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            await Task.Yield();
            context.Console.Output.Write($"async:{greeting.Greeting}");
            return 0;
        }
    }
}
