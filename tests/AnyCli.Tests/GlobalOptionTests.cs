// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class GlobalOptionTests
{
    [Test]
    public async Task GlobalOption_BeforeCommand_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug|");
    }

    [Test]
    public async Task GlobalOption_AfterCommand_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--log-level", "debug"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug|");
    }

    [Test]
    public async Task GlobalOption_SupportsEqualsAndShortAliases()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--log-level=warning"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("warning|");
    }

    [Test]
    public async Task GlobalOption_SupportsShortAlias()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["-l", "warning", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("warning|");
    }

    [Test]
    public async Task GlobalCollectionOption_CollectsRepeatedValues()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddCollection<string>("tag").SetDescription("Tag"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--tag", "one", "--tag", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("none|one,two");
    }

    [Test]
    public async Task TypedCommandParameters_AndGlobalOptions_WorkTogether()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalTypedCommand, GlobalTypedParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["--log-level", "debug", "run", "hello", "--verbose"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello:True:debug");
    }

    [Test]
    public async Task GlobalOptions_CanBeConfiguredAfterCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug|");
    }

    [Test]
    public async Task GlobalCollectionOptionDefault_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options =>
                options.AddCollection<string>("tag").SetDefaultValue(new[] { "a", "b" }))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("none|a,b");
    }

    [Test]
    public async Task GlobalOptionDefault_IsAvailableOnContext()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options =>
                options.AddValue<string>("log-level").SetDefaultValue("information"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("information|");
    }

    [Test]
    public async Task ExplicitGlobalOption_OverridesDefault()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options =>
                options.AddValue<string>("log-level").SetDefaultValue("information"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug|");
    }

    [Test]
    public async Task RequiredGlobalOption_IsEnforced()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level").SetRequired())
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        CommandParameterException? exception = null;

        try
        {
            _ = await TestAppHarness.RunAsync(app, ["run"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'--log-level'");
    }

    [Test]
    public async Task RequiredGlobalOption_IsSatisfiedBeforeCommand()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level").SetRequired())
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--log-level", "debug", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("debug|");
    }

    [Test]
    public async Task Help_IncludesGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .AddHelp<GlobalHelpProvider>()
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
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level").SetRequired())
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .AddHelp<GlobalHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("--log-level");
    }

    [Test]
    public async Task UnknownGlobalOption_BeforeCommand_Throws()
    {
        var app = ConsoleAppBuilder.Create()
            .WithGlobalOptions(options => options.AddValue<string>("log-level").SetAliases("l").SetDescription("Log level"))
            .ConfigureCommands(commands => commands.AddCommand<GlobalEchoCommand>("run"))
            .Build();

        CommandParameterException? exception = null;

        try
        {
            _ = await TestAppHarness.RunAsync(app, ["--verbose", "run"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("--verbose");
    }

    [Test]
    public async Task GlobalOptionConflictingWithCommandOption_ThrowsAtBuild()
    {
        InvalidOperationException? exception = null;

        try
        {
            _ = ConsoleAppBuilder.Create()
                .WithGlobalOptions(options => options.AddValue<string>("log-level"))
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
                .WithGlobalOptions(options => options.AddFlag("help"))
                .AddHelp<GlobalHelpProvider>()
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
    public async Task DuplicateGlobalOption_ThrowsAtBuild()
    {
        InvalidOperationException? exception = null;

        try
        {
            _ = ConsoleAppBuilder.Create()
                .WithGlobalOptions(options => options.AddValue<string>("log-level"))
                .WithGlobalOptions(options => options.AddValue<string>("log-level"))
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("--log-level");
    }

    private sealed class GlobalEchoCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            var logLevel = context.GlobalOptionValues.TryGetValue("log-level", out var value)
                ? value
                : "none";
            var tags = string.Join(",", context.GlobalOptionValues.GetValues("tag"));
            context.Console.Output.Write($"{logLevel}|{tags}");
            return 0;
        }
    }

    private sealed class GlobalTypedCommand : ICommand<GlobalTypedParameters>
    {
        public int Execute(CommandContext context, GlobalTypedParameters parameters)
        {
            var logLevel = context.GlobalOptionValues.TryGetValue("log-level", out var value)
                ? value
                : "none";
            context.Console.Output.Write($"{parameters.Input}:{parameters.Verbose}:{logLevel}");
            return 0;
        }
    }

    private sealed class GlobalConflictCommand : ICommand<GlobalConflictParameters>
    {
        public int Execute(CommandContext context, GlobalConflictParameters parameters) => 0;
    }

    private sealed class GlobalHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context) =>
            WriteOptions(context.GlobalOptions, context.Console);

        public void WriteCommandHelp(CommandHelpContext context) =>
            WriteOptions(context.GlobalOptions, context.Console);

        private static void WriteOptions(
            IReadOnlyList<OptionInfo> options,
            IConsoleWriter console) =>
            console.Output.Write(string.Join(",", options.SelectMany(option => option.Names)));
    }
}

public sealed partial class GlobalTypedParameters
{
    [Argument(0)]
    public string Input { get; set; } = "";

    [Option("verbose")]
    public bool Verbose { get; set; }
}

public sealed partial class GlobalConflictParameters
{
    [Option("log-level")]
    public string LogLevel { get; set; } = "";
}
