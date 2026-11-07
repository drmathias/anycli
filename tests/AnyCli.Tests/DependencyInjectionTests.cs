// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class DependencyInjectionTests
{
    [Test]
    public async Task Command_IsResolvedFromProvider()
    {
        var serviceProvider = new TestServiceProvider();
        serviceProvider.Add(() => new GreetCommand(new GreetingService("hello")));

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .AddCommand<GreetCommand>("greet")
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(["greet"], output, error);

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(output.ToString()).IsEqualTo("hello");
        await Assert.That(error.ToString()).IsEmpty();
    }

    [Test]
    public async Task AsyncCommand_IsResolvedFromProvider()
    {
        var serviceProvider = new TestServiceProvider();
        serviceProvider.Add(() => new AsyncGreetCommand(new GreetingService("hello")));

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .AddAsyncCommand<AsyncGreetCommand>("greet")
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(["greet"], output, error);

        await Assert.That(exitCode).IsEqualTo(0);
        await Assert.That(output.ToString()).IsEqualTo("async:hello");
        await Assert.That(error.ToString()).IsEmpty();
    }

    [Test]
    public async Task Command_ResolvesNewInstancePerRun()
    {
        var created = 0;
        var serviceProvider = new TestServiceProvider();
        serviceProvider.Add(() =>
        {
            created++;
            return new GreetCommand(new GreetingService("hello"));
        });

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .AddCommand<GreetCommand>("greet")
            .Build();

        await app.RunAsync(["greet"], new StringWriter(), new StringWriter());
        await app.RunAsync(["greet"], new StringWriter(), new StringWriter());

        await Assert.That(created).IsEqualTo(2);
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, Func<object>> _factories = [];

        public void Add<TService>(Func<TService> factory)
            where TService : class =>
            _factories[typeof(TService)] = factory;

        public object? GetService(Type serviceType) =>
            _factories.TryGetValue(serviceType, out var factory) ? factory() : null;
    }

    private sealed class GreetingService(string greeting)
    {
        public string Greeting { get; } = greeting;
    }

    private sealed class GreetCommand(GreetingService greeting) : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Output.Write(greeting.Greeting);
            return 0;
        }
    }

    private sealed class AsyncGreetCommand(GreetingService greeting) : IAsyncCommand<GreetExitCode>
    {
        public async Task<GreetExitCode> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            context.Output.Write($"async:{greeting.Greeting}");
            return GreetExitCode.Success;
        }
    }

    private enum GreetExitCode
    {
        Success = 0,
    }
}
