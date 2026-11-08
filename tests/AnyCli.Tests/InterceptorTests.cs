// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class InterceptorTests
{
    [Test]
    public async Task Interceptors_RunInRegistrationOrder()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddCommand<OutputCommand>("run"))
            .AddInterceptor<FirstInterceptor>()
            .AddInterceptor<SecondInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("A>B>command<B<A");
    }

    [Test]
    public async Task Interceptor_CanReadGlobalOptionValues()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level"))
            .ConfigureCommands(commands => commands.AddCommand<OutputCommand>("run"))
            .AddInterceptor<GlobalOptionInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug|command");
    }

    [Test]
    public async Task Interceptor_CanShortCircuitExecution()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddCommand<OutputCommand>("run"))
            .AddInterceptor<ShortCircuitInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(7);
        await Assert.That(result.Output).IsEqualTo("short-circuit");
    }

    [Test]
    public async Task Interceptor_CanModifyExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddCommand<FailCommand>("run"))
            .AddInterceptor<AddTenInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(11);
    }

    [Test]
    public async Task Interceptors_WrapAsyncCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddAsyncCommand<AsyncOutputCommand>("run"))
            .AddInterceptor<FirstInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("A>async-command<A");
    }

    [Test]
    public async Task Interceptor_IsResolvedFromServiceProvider()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new InterceptorService("injected"));
        serviceProvider.Add(() => new InjectedInterceptor(new InterceptorService("injected")));
        serviceProvider.Add(() => new OutputCommand());

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands => commands.AddCommand<OutputCommand>("run"))
            .AddInterceptor<InjectedInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("injected|command");
    }

    [Test]
    public async Task TypedInterceptor_ReceivesAndModifiesEnumExitCode()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands<InterceptorExitCode>(commands =>
                commands.AddCommand<EnumFailCommand>("fail"))
            .AddInterceptor<EnumInterceptor>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["fail"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)InterceptorExitCode.Succeeded);
        await Assert.That(result.Output).IsEqualTo("typed>after:Failed<");
    }

    [Test]
    public async Task Interceptors_DoNotRunForFrameworkLevelOutcomes()
    {
        var app = ConsoleAppBuilder.Create()
            .WithVersion("1.0.0")
            .ConfigureCommands(commands => commands.AddCommand<OutputCommand>("run"))
            .AddHelp<NoopHelpProvider>()
            .AddInterceptor<MarkerInterceptor>()
            .Build();

        var version = await TestAppHarness.RunAsync(app, ["--version"]);
        var help = await TestAppHarness.RunAsync(app, ["--help"]);
        var unknown = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(version.Output).IsEqualTo($"1.0.0{Environment.NewLine}");
        await Assert.That(help.Output).IsEqualTo("help");
        await Assert.That(unknown.ExitCode).IsEqualTo(2);

        await Assert.That(version.Output).DoesNotContain("intercepted");
        await Assert.That(help.Output).DoesNotContain("intercepted");
        await Assert.That(unknown.Error).DoesNotContain("intercepted");
    }

    private sealed class OutputCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write("command");
            return 0;
        }
    }

    private sealed class AsyncOutputCommand : NoParametersAsyncCommand
    {
        public override Task<int> ExecuteAsync(CommandContext context)
        {
            context.Console.Output.Write("async-command");
            return Task.FromResult(0);
        }
    }

    private sealed class FailCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context) => 1;
    }

    private sealed class EnumFailCommand : NoParametersCommand<InterceptorExitCode>
    {
        public override InterceptorExitCode Execute(CommandContext context) =>
            InterceptorExitCode.Failed;
    }

    private sealed class FirstInterceptor : ICommandInterceptor
    {
        public async Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
        {
            context.Console.Output.Write("A>");
            var exitCode = await next().ConfigureAwait(false);
            context.Console.Output.Write("<A");
            return exitCode;
        }
    }

    private sealed class SecondInterceptor : ICommandInterceptor
    {
        public async Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
        {
            context.Console.Output.Write("B>");
            var exitCode = await next().ConfigureAwait(false);
            context.Console.Output.Write("<B");
            return exitCode;
        }
    }

    private sealed class GlobalOptionInterceptor : ICommandInterceptor
    {
        public Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
        {
            var logLevel = context.GlobalOptionValues.TryGetValue("log-level", out var value)
                ? value
                : "none";
            context.Console.Output.Write($"{logLevel}|");
            return next();
        }
    }

    private sealed class ShortCircuitInterceptor : ICommandInterceptor
    {
        public Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
        {
            context.Console.Output.Write("short-circuit");
            return Task.FromResult(7);
        }
    }

    private sealed class AddTenInterceptor : ICommandInterceptor
    {
        public async Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next) =>
            await next().ConfigureAwait(false) + 10;
    }

    private sealed class EnumInterceptor : ICommandInterceptor<InterceptorExitCode>
    {
        public async Task<InterceptorExitCode> InterceptAsync(
            CommandContext context,
            Func<Task<InterceptorExitCode>> next)
        {
            context.Console.Output.Write("typed>");
            var exitCode = await next().ConfigureAwait(false);
            context.Console.Output.Write($"after:{exitCode}<");
            return InterceptorExitCode.Succeeded;
        }
    }

    private sealed class MarkerInterceptor : ICommandInterceptor
    {
        public Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
        {
            context.Console.Output.Write("intercepted");
            return next();
        }
    }

    private sealed record InterceptorService(string Message);

    private sealed class InjectedInterceptor(InterceptorService service) : ICommandInterceptor
    {
        public Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
        {
            context.Console.Output.Write($"{service.Message}|");
            return next();
        }
    }

    private enum InterceptorExitCode
    {
        None = 0,
        Succeeded = 1,
        Failed = 4,
    }

    private sealed class NoopHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context) => context.Console.Output.Write("help");

        public void WriteCommandHelp(CommandHelpContext context) => context.Console.Output.Write("help");
    }
}
