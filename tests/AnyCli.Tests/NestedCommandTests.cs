// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class NestedCommandTests
{
    [Test]
    public async Task NestedCommand_ExecutesWithFullPath()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(
            app,
            ["remote", "add", "origin", "--loud"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote.add|origin|True|none");
    }

    [Test]
    public async Task DeeplyNestedCommand_ExecutesWithFullPath()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["remote", "config", "set"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote.config.set");
    }

    [Test]
    [Arguments("--log-level", "debug", "remote", "add", "origin")]
    [Arguments("remote", "--log-level", "debug", "add", "origin")]
    [Arguments("remote", "add", "--log-level", "debug", "origin")]
    public async Task GlobalOptions_WorkAroundCommandPaths(params string[] args)
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, args);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("debug");
    }

    [Test]
    public async Task RootHelp_IncludesGroupsAndCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<NestedPathCommand>("ping", "Ping the service");
                commands.AddGroup("remote", "Manage remotes", group =>
                {
                    group.AddCommand<NestedPathCommand>("add", "Add a remote");
                });
            })
            .AddHelp<TreeHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            "ping|True|Ping the service|\n" +
            "remote|False|Manage remotes|add");
    }

    [Test]
    public async Task GroupHelp_ExposesGroupAndChildren()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
            {
                commands.AddGroup("remote", "Manage remotes", group =>
                {
                    group.AddCommand<NestedPathCommand>("add", "Add a remote");
                    group.AddCommand<NestedPathCommand>("remove", "Remove a remote");
                });
            })
            .AddHelp<TreeHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["remote", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            "remote|False|Manage remotes|add,remove");
    }

    [Test]
    public async Task LeafHelp_ExposesFullPath()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
            {
                commands.AddGroup("remote", "Manage remotes", group =>
                {
                    group.AddCommand<NestedPathCommand>("add", "Add a remote");
                });
            })
            .AddHelp<TreeHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["remote", "add", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote.add|True|Add a remote|");
    }

    [Test]
    public async Task MissingSubcommand_ReportsRequiresSubcommand()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["remote"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Error).IsEqualTo(
            $"Command 'remote' requires a subcommand.{Environment.NewLine}");
    }

    [Test]
    public async Task UnknownNestedCommand_ReportsFullPath()
    {
        var app = CreateApp();

        var result = await TestAppHarness.RunAsync(app, ["remote", "nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Error).IsEqualTo(
            $"Unknown command 'remote nope'.{Environment.NewLine}");
    }

    [Test]
    public async Task DuplicateCommandName_Throws()
    {
        InvalidOperationException? exception = null;

        try
        {
            _ = ConsoleAppBuilder.Create()
                .ConfigureCommands(commands =>
                {
                    commands.AddCommand<NestedPathCommand>("remote");
                    commands.AddGroup("remote", "Manage remotes", _ => { });
                })
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("remote");
    }

    [Test]
    public async Task EnumExitCodeGroup_Works()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands<NestedExitCode>(commands =>
            {
                commands.AddGroup("remote", "Manage remotes", group =>
                {
                    group.AddCommand<NestedEnumCommand>("fail", "Fail");
                });
            })
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["remote", "fail"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)NestedExitCode.Failed);
    }

    [Test]
    public async Task ExecutableGroup_RunsParentWhenNoSubcommandIsSpecified()
    {
        var app = CreateExecutableGroupApp();

        var result = await TestAppHarness.RunAsync(app, ["remote"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote|False");
    }

    [Test]
    public async Task ExecutableGroup_RunsParentOptions()
    {
        var app = CreateExecutableGroupApp();

        var result = await TestAppHarness.RunAsync(app, ["remote", "--verbose"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote|True");
    }

    [Test]
    public async Task ExecutableGroup_RunsChildWhenSubcommandIsSpecified()
    {
        var app = CreateExecutableGroupApp();

        var result = await TestAppHarness.RunAsync(app, ["remote", "add"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote.add");
    }

    [Test]
    public async Task ExecutableGroup_GlobalOptionsCanPrecedeChild()
    {
        var app = ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<TypedExecutableGroupCommand, ExecutableGroupParameters>(
                    "remote",
                    "Manage remotes",
                    group => group.AddCommand<TypedNestedPathCommand, TypedNestedPathParameters>("add", "Add a remote"));
            })
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["remote", "--log-level", "debug", "add"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote.add");
    }

    [Test]
    public async Task ExecutableGroup_ReportsUnknownSubcommand()
    {
        var app = CreateExecutableGroupApp();

        var result = await TestAppHarness.RunAsync(app, ["remote", "nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Error).IsEqualTo(
            $"Unknown command 'remote nope'.{Environment.NewLine}");
    }

    [Test]
    public async Task ExecutableGroup_HelpIncludesCommandAndChildren()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<ExecutableGroupCommand, ExecutableGroupParameters>(
                    "remote",
                    "Manage remotes",
                    group => group.AddCommand<NestedPathCommand>("add", "Add a remote"));
            })
            .AddHelp<ExecutableGroupHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["remote", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("remote|True|True|1|add");
    }

    [Test]
    public async Task ExecutableGroup_RejectsPositionalArguments()
    {
        InvalidOperationException? exception = null;

        try
        {
            _ = ConsoleAppBuilder.Create()
                .ConfigureCommands(commands =>
                {
                    commands.AddCommand<ExecutableGroupWithArgumentCommand, ExecutableGroupWithArgumentParameters>(
                        "remote",
                        "Manage remotes",
                        group => group.AddCommand<NestedPathCommand>("add", "Add a remote"));
                })
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("remote");
        await Assert.That(exception.Message).Contains("positional arguments");
    }

    private static ConsoleApp CreateExecutableGroupApp() =>
        ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<ExecutableGroupCommand, ExecutableGroupParameters>(
                    "remote",
                    "Manage remotes",
                    group => group.AddCommand<NestedPathCommand>("add", "Add a remote"));
            })
            .Build();

    private static ConsoleApp CreateApp() =>
        ConsoleAppBuilder.Create()
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands =>
            {
                commands.AddGroup("remote", "Manage remotes", group =>
                {
                    group.AddCommand<NestedEchoCommand, NestedEchoParameters>("add", "Add a remote");
                    group.AddGroup("config", "Configure remotes", nested =>
                    {
                        nested.AddCommand<TypedNestedPathCommand, TypedNestedPathParameters>("set", "Set a value");
                    });
                });
            })
            .Build();

    private sealed class NestedEchoCommand : ICommand<NestedEchoParameters>
    {
        public int Execute(CommandContext context, NestedEchoParameters parameters)
        {
            context.Console.Output.Write(
                $"{string.Join(".", context.CommandPath)}|{parameters.Value}|{parameters.Loud}|{parameters.LogLevel}");
            return 0;
        }
    }

    private sealed class TypedNestedPathCommand : ICommand<TypedNestedPathParameters>
    {
        public int Execute(CommandContext context, TypedNestedPathParameters parameters)
        {
            context.Console.Output.Write(string.Join(".", context.CommandPath));
            return 0;
        }
    }

    private sealed class TypedExecutableGroupCommand : ICommand<ExecutableGroupParameters>
    {
        public int Execute(CommandContext context, ExecutableGroupParameters parameters)
        {
            context.Console.Output.Write(
                $"{string.Join(".", context.CommandPath)}|{parameters.Verbose}");
            return 0;
        }
    }

    private sealed class NestedPathCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write(string.Join(".", context.CommandPath));
            return 0;
        }
    }

    private sealed class NestedEnumCommand : NoParametersCommand<NestedExitCode>
    {
        public override NestedExitCode Execute(CommandContext context) => NestedExitCode.Failed;
    }

    private sealed class ExecutableGroupCommand : ICommand<ExecutableGroupParameters>
    {
        public int Execute(CommandContext context, ExecutableGroupParameters parameters)
        {
            context.Console.Output.Write(
                $"{string.Join(".", context.CommandPath)}|{parameters.Verbose}");
            return 0;
        }
    }

    private sealed class ExecutableGroupWithArgumentCommand : ICommand<ExecutableGroupWithArgumentParameters>
    {
        public int Execute(CommandContext context, ExecutableGroupWithArgumentParameters parameters) => 0;
    }

    private sealed class ExecutableGroupHelpProvider : IHelpProvider
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write("root");
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            var command = context.Command;
            context.Console.Output.Write(
                $"{string.Join(".", command.Path)}|{command.IsExecutable}|{command.HasSubcommands}|" +
                $"{command.Options.Count}|{string.Join(",", command.Children.Select(child => child.Name))}");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TreeHelpProvider : IHelpProvider
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            Write(context.Commands, context.Console);
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            Write([context.Command], context.Console);
            return ValueTask.CompletedTask;
        }

        private static void Write(
            IReadOnlyList<CommandInfo> commands,
            IConsoleWriter console) =>
            console.Output.Write(string.Join("\n", commands.Select(Describe)));

        private static string Describe(CommandInfo command) =>
            $"{string.Join(".", command.Path)}|{command.IsExecutable}|{command.Description}|" +
            $"{string.Join(",", command.Children.Select(child => child.Name))}";
    }
}

public sealed partial class NestedEchoParameters : TestGlobalOptions
{
    [Argument(0)]
    public string Value { get; set; } = "";

    [Option("loud")]
    public bool Loud { get; set; }
}

public sealed partial class ExecutableGroupParameters : TestGlobalOptions
{
    [Option("verbose")]
    public bool Verbose { get; set; }
}

public sealed partial class ExecutableGroupWithArgumentParameters
{
    [Argument(0)]
    public string Name { get; set; } = "";
}

public sealed partial class TypedNestedPathParameters : TestGlobalOptions
{
}

public enum NestedExitCode
{
    Failed = 4,
}
