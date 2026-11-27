using System.ComponentModel;

// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class GlobalOptionTests
{
    [Test]
    public async Task GlobalOption_BeforeCommand_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug||False");
    }

    [Test]
    public async Task GlobalOption_AfterCommand_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--log-level", "debug"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug||False");
    }

    [Test]
    public async Task GlobalOption_SupportsEqualsAndShortAliases()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--log-level=warning"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("warning||False");
    }

    [Test]
    public async Task GlobalOption_SupportsShortAlias()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["-l", "warning", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("warning||False");
    }

    [Test]
    public async Task GlobalCollectionOption_CollectsRepeatedValues()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--tag", "one", "--tag", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("none|one,two|False");
    }

    [Test]
    public async Task RepeatedGlobalScalarOption_WritesError()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["--log-level", "debug", "--log-level", "warning", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("'--log-level'");
        await Assert.That(result.Error).Contains("more than once");
    }

    [Test]
    public async Task GlobalFlag_IsAvailableOnTypedContext()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--quiet", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("none||True");
    }

    [Test]
    public async Task TypedCommandParameters_AndGlobalOptions_WorkTogether()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalTypedCommand, GlobalTypedParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["--log-level", "debug", "run", "hello", "--verbose"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello:True:debug");
    }

    [Test]
    public async Task GlobalCollectionOptionDefault_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<CollectionDefaultGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<CollectionDefaultEchoCommand, CollectionDefaultEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("a,b");
    }

    [Test]
    public async Task GlobalOptionDefault_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<DefaultGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<DefaultEchoCommand, DefaultEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("information");
    }

    [Test]
    public async Task ExplicitGlobalOption_OverridesDefault()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<DefaultGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<DefaultEchoCommand, DefaultEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug");
    }

    [Test]
    public async Task GlobalOptionDefaultProvider_IsUsedOnce()
    {
        var provider = new GlobalLevelProvider("provider");
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => provider);
        serviceProvider.Add(() => new ProviderGlobalEchoCommand());

        var app = CreateProviderApp(serviceProvider);

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("provider|provider");
        await Assert.That(provider.Invocations).IsEqualTo(1);
    }

    [Test]
    public async Task GlobalOptionDefaultProvider_YieldsToSuppliedValue()
    {
        var provider = new GlobalLevelProvider("provider");
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => provider);
        serviceProvider.Add(() => new ProviderGlobalEchoCommand());

        var app = CreateProviderApp(serviceProvider);

        var result = await TestAppHarness.RunAsync(app, ["--level", "cli", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("cli|cli");
        await Assert.That(provider.Invocations).IsEqualTo(0);
    }

    [Test]
    public async Task AsyncGlobalOptionDefaultProvider_IsAwaited()
    {
        var provider = new AsyncGlobalLevelProvider("async-provider");
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => provider);
        serviceProvider.Add(() => new AsyncProviderGlobalEchoCommand());

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .AddGlobalOptions<AsyncProviderGlobalOptions>()
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncProviderGlobalEchoCommand, AsyncProviderGlobalParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("async-provider|async-provider");
        await Assert.That(provider.Invocations).IsEqualTo(1);
    }

    [Test]
    public async Task GlobalOptionDefaultProvider_IsNotInvokedForHelp()
    {
        var provider = new GlobalLevelProvider("provider");
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => provider);
        serviceProvider.Add(() => new ProviderGlobalEchoCommand());

        var app = CreateProviderApp(serviceProvider);

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("--level");
        await Assert.That(provider.Invocations).IsEqualTo(0);
    }

    private static ConsoleApp CreateProviderApp(StubServiceProvider serviceProvider) =>
        ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .AddGlobalOptions<ProviderGlobalOptions>()
            .ConfigureCommands(commands =>
                commands.AddCommand<ProviderGlobalEchoCommand, ProviderGlobalParameters>("run"))
            .Build();

    [Test]
    public async Task RequiredGlobalOption_IsEnforced()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<RequiredGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<RequiredEchoCommand, RequiredEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("'--log-level'");
    }

    [Test]
    public async Task RequiredGlobalOption_IsSatisfiedBeforeCommand()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<RequiredGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<RequiredEchoCommand, RequiredEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug");
    }

    [Test]
    public async Task Help_IncludesGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureHelp(help => help.UseProvider<GlobalHelpProvider>())
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("--log-level");
        await Assert.That(result.Output).Contains("--help");
    }

    [Test]
    public async Task CommandHelp_BypassesRequiredGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<RequiredGlobalOptions>()
            .ConfigureHelp(help => help.UseProvider<GlobalHelpProvider>())
            .ConfigureCommands(commands => commands.AddCommand<RequiredEchoCommand, RequiredEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("--log-level");
    }

    [Test]
    public async Task UnknownGlobalOption_BeforeCommand_WritesError()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--unknown", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("--unknown");
    }

    [Test]
    public async Task GlobalOptionConflictingWithCommandOption_ThrowsAtBuild()
    {
        InvalidOperationException? exception = null;

        try
        {
            _ = ConsoleAppBuilder.Create()
                .AddGlobalOptions<TestGlobalOptions>()
                .ConfigureCommands(commands => commands.AddCommand<GlobalConflictCommand, GlobalConflictParameters>("run"))
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("--log-level");
    }

    [Test]
    public async Task GlobalOptionConflictingWithBuiltIn_ThrowsAtBuild()
    {
        InvalidOperationException? exception = null;

        try
        {
            _ = ConsoleAppBuilder.Create()
                .AddGlobalOptions<BuiltInConflictGlobalOptions>()
                .ConfigureHelp(help => help.UseProvider<GlobalHelpProvider>())
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("--help");
    }

    [Test]
    public async Task AddGlobalOptions_CannotBeCalledTwice()
    {
        var builder = ConsoleAppBuilder.Create();
        _ = builder.AddGlobalOptions<TestGlobalOptions>();

        InvalidOperationException? exception = null;

        try
        {
            _ = builder.AddGlobalOptions<TestGlobalOptions>();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("already been configured");
    }

    [Test]
    public async Task Build_RejectsCommandParametersThatDoNotDeriveFromGlobalOptions()
    {
        InvalidOperationException? exception = null;

        try
        {
            var builder = ConsoleAppBuilder.Create();
            builder.ConfigureCommands(commands =>
                commands.AddCommand<NonGlobalCommand, NonGlobalParameters>("run"));
            _ = builder.AddGlobalOptions<TestGlobalOptions>().Build();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("must derive from");
    }

    [Test]
    public async Task AsyncCommand_ReceivesTypedGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddAsyncCommand<GlobalAsyncCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug");
    }

    [Test]
    public async Task ExitCodeCommand_ReceivesTypedGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands<GlobalExitCode>(commands => commands.AddCommand<GlobalExitCodeCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(3);
        await Assert.That(result.Output).IsEqualTo("debug");
    }

    [Test]
    public async Task NestedGroup_ReceivesTypedGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddGroup(
                "group",
                "A group",
                group => group.AddCommand<GlobalEchoCommand, TestGlobalEchoParameters>("run")))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "group", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug||False");
    }

    private sealed class GlobalEchoCommand : ICommand<TestGlobalEchoParameters>
    {
        public int Execute(CommandContext context, TestGlobalEchoParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.LogLevel}|{string.Join(",", parameters.Tags)}|{parameters.Quiet}");
            return 0;
        }
    }

    private sealed class GlobalTypedCommand : ICommand<GlobalTypedParameters>
    {
        public int Execute(CommandContext context, GlobalTypedParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Input}:{parameters.Verbose}:{parameters.LogLevel}");
            return 0;
        }
    }

    private sealed class GlobalConflictCommand : ICommand<GlobalConflictParameters>
    {
        public int Execute(CommandContext context, GlobalConflictParameters parameters) => 0;
    }

    private sealed class NonGlobalCommand : ICommand<NonGlobalParameters>
    {
        public int Execute(CommandContext context, NonGlobalParameters parameters) => 0;
    }

    private sealed class GlobalAsyncCommand : IAsyncCommand<TestGlobalEchoParameters>
    {
        public Task<int> ExecuteAsync(CommandContext context, TestGlobalEchoParameters parameters, CancellationToken cancellationToken)
        {
            context.Console.Output.Write(parameters.LogLevel);
            return Task.FromResult(0);
        }
    }

    private sealed class GlobalExitCodeCommand : ICommand<TestGlobalEchoParameters, GlobalExitCode>
    {
        public GlobalExitCode Execute(CommandContext context, TestGlobalEchoParameters parameters)
        {
            context.Console.Output.Write(parameters.LogLevel);
            return GlobalExitCode.Failed;
        }
    }

    private sealed class DefaultEchoCommand : ICommand<DefaultEchoParameters>
    {
        public int Execute(CommandContext context, DefaultEchoParameters parameters)
        {
            context.Console.Output.Write(parameters.LogLevel);
            return 0;
        }
    }

    private sealed class RequiredEchoCommand : ICommand<RequiredEchoParameters>
    {
        public int Execute(CommandContext context, RequiredEchoParameters parameters)
        {
            context.Console.Output.Write(parameters.LogLevel);
            return 0;
        }
    }

    private sealed class AsyncProviderGlobalEchoCommand : ICommand<AsyncProviderGlobalParameters>
    {
        public int Execute(CommandContext context, AsyncProviderGlobalParameters parameters)
        {
            var globalOptions = ((CommandContext<AsyncProviderGlobalOptions>)context).GlobalOptions;
            context.Console.Output.Write($"{globalOptions.Level}|{parameters.Level}");
            return 0;
        }
    }

    private sealed class ProviderGlobalEchoCommand : ICommand<ProviderGlobalParameters>
    {
        public int Execute(CommandContext context, ProviderGlobalParameters parameters)
        {
            var globalOptions = ((CommandContext<ProviderGlobalOptions>)context).GlobalOptions;
            context.Console.Output.Write($"{globalOptions.Level}|{parameters.Level}");
            return 0;
        }
    }

    private sealed class CollectionDefaultEchoCommand : ICommand<CollectionDefaultEchoParameters>
    {
        public int Execute(CommandContext context, CollectionDefaultEchoParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Tags));
            return 0;
        }
    }

    private sealed class GlobalHelpProvider : IHelpProvider
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            WriteOptions(context.Application.GlobalOptions, context.Console);
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            WriteOptions(context.Application.GlobalOptions, context.Console);
            return ValueTask.CompletedTask;
        }

        private static void WriteOptions(
            IReadOnlyList<OptionInfo> options,
            IConsoleWriter console) =>
            console.Output.Write(string.Join(",", options.SelectMany(option => option.Names)));
    }

    private enum GlobalExitCode
    {
        Success = 0,
        Failed = 3,
    }
}

[GlobalOptions]
public partial class TestGlobalOptions
{
    [Option("--log-level", "-l")]
    [Description("Log level")]
    public string LogLevel { get; set; } = "none";

    [Option("--tag")]
    [Description("Tag")]
    public string[] Tags { get; set; } = [];

    [Option("--quiet")]
    public bool Quiet { get; set; }
}

[GlobalOptions]
public partial class DefaultGlobalOptions
{
    [Option("--log-level")]
    public string LogLevel { get; set; } = "information";
}

[GlobalOptions]
public partial class RequiredGlobalOptions
{
    [RequiredOption("--log-level")]
    public string LogLevel { get; set; } = "";
}

[GlobalOptions]
public partial class CollectionDefaultGlobalOptions
{
    [Option("--tag")]
    public string[] Tags { get; set; } = ["a", "b"];
}

[GlobalOptions]
public partial class BuiltInConflictGlobalOptions
{
    [Option("--help")]
    public bool Help { get; set; }
}

public sealed partial class TestGlobalEchoParameters : TestGlobalOptions
{
}

public sealed partial class DefaultEchoParameters : DefaultGlobalOptions
{
}

public sealed partial class RequiredEchoParameters : RequiredGlobalOptions
{
}

public sealed partial class CollectionDefaultEchoParameters : CollectionDefaultGlobalOptions
{
}

public sealed partial class GlobalTypedParameters : TestGlobalOptions
{
    [Argument(0)]
    public string Input { get; set; } = "";

    [Option("--verbose")]
    public bool Verbose { get; set; }
}

public sealed partial class GlobalConflictParameters : TestGlobalOptions
{
    [Option("--log-level")]
    public new string LogLevel { get; set; } = "";
}

public sealed partial class NonGlobalParameters
{
    [Argument(0)]
    public string Value { get; set; } = "";
}


[GlobalOptions]
public partial class ProviderGlobalOptions
{
    [Option("--level", DefaultValueProvider = typeof(GlobalLevelProvider))]
    public string Level { get; set; } = "initializer";
}

public sealed partial class ProviderGlobalParameters : ProviderGlobalOptions
{
}

[GlobalOptions]
public partial class AsyncProviderGlobalOptions
{
    [Option("--level", DefaultValueProvider = typeof(AsyncGlobalLevelProvider))]
    public string Level { get; set; } = "initializer";
}

public sealed partial class AsyncProviderGlobalParameters : AsyncProviderGlobalOptions
{
}

public sealed class GlobalLevelProvider(string value) : IDefaultValueProvider<string>
{
    public int Invocations { get; private set; }

    public string CreateDefault()
    {
        Invocations++;
        return value;
    }
}

public sealed class AsyncGlobalLevelProvider(string value) : IAsyncDefaultValueProvider<string>
{
    public int Invocations { get; private set; }

    public ValueTask<string> CreateDefaultAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Invocations++;
        return ValueTask.FromResult(value);
    }
}
