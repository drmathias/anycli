// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class HelpCommandTests
{
    private const string ExpectedRootHelp =
        """
        Usage:
            jvln [OPTIONS] <COMMAND>

        Options:
            -h, --help     Prints help information
            -v, --version  Prints version information

        Commands:
            arm       Starts the Jvln daemon
            disarm    Stops the Jvln daemon
            status    Retrives the current status of the Jvln daemon
            target    Establishes a secure connection with a Jvln server
            throw     Opens a Jvln tunnel
            retrieve  Closes a Jvln tunnel
        """;

    private const string ExpectedThrowHelp =
        """
        Description:
            Opens a Jvln HTTP tunnel

        Usage:
            jvln throw <upstream> [OPTIONS]

        Arguments:
            <upstream>  Destination for HTTP traffic

        Options:
            -h, --help                          Prints help information
        """;

    [Test]
    [Arguments("-h")]
    [Arguments("--help")]
    public async Task RootHelp_InvokesProviderWithAllCommands(string flag)
    {
        var result = await Run([flag]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedRootHelp));
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    [Arguments("-h")]
    [Arguments("--help")]
    public async Task CommandHelp_InvokesProviderWithCommandName(string flag)
    {
        var result = await Run(["arm", flag]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("arm: Starts the Jvln daemon");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    [Arguments("-h")]
    [Arguments("--help")]
    public async Task CommandHelp_IncludesArgumentMetadata(string flag)
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("jvln")
            .ConfigureCommands(commands => commands.AddCommand<ThrowCommand, ThrowParameters>(
                "throw",
                "Opens a Jvln HTTP tunnel"))
            .AddHelp<ThrowHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["throw", flag]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(Normalize(result.Output)).IsEqualTo(Normalize(ExpectedThrowHelp));
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task CommandHelp_ExposesArgumentTypes()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TypeCommand, TypeMetadataParameters>("types"))
            .AddHelp<TypeHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["types", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            $"{typeof(int).Name}:1:1:True:False," +
            $"{typeof(int?).Name}:0:1:False:False," +
            $"{typeof(string[]).Name}:0:{int.MaxValue}:False:True");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task CommandHelp_ExposesOneOrMoreArity()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<OneOrMoreCommand, OneOrMoreParameters>("tags"))
            .AddHelp<TypeHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["tags", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            $"{typeof(string[]).Name}:1:{int.MaxValue}:True:True");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task CommandHelp_ExposesCommandOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<OptionMetadataCommand, OptionParameters>("opts"))
            .AddHelp<OptionHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, "opts", "--help");

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            "--config:True:True|--output,-o:False:True|--verbose,-v:False:False|" +
            "--quiet,-q:False:False|--tag:False:True|--count:False:True|--dry-run:False:False");
        await Assert.That(result.Error).IsEmpty();
    }

    private static async Task<TestAppResult> Run(string[] args)
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("jvln")
            .WithVersion("1.0.0")
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<NoOpCommand>("arm", "Starts the Jvln daemon");
                commands.AddCommand<NoOpCommand>("disarm", "Stops the Jvln daemon");
                commands.AddCommand<NoOpCommand>("status", "Retrives the current status of the Jvln daemon");
                commands.AddCommand<NoOpCommand>("target", "Establishes a secure connection with a Jvln server");
                commands.AddCommand<NoOpCommand>("throw", "Opens a Jvln tunnel");
                commands.AddCommand<NoOpCommand>("retrieve", "Closes a Jvln tunnel");
            })
            .AddHelp<StubHelpProvider>()
            .Build();

        return await TestAppHarness.RunAsync(app, args);
    }

    private static string Normalize(string value) =>
        string.Join(
            '\n',
            value.ReplaceLineEndings("\n")
                .Split('\n')
                .Select(line => line.TrimEnd()))
            .TrimEnd('\n');

    private sealed class NoOpCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context) => 0;
    }

    private sealed class OptionMetadataCommand : ICommand<OptionParameters>
    {
        public int Execute(CommandContext context, OptionParameters parameters) => 0;
    }

    private sealed class OptionHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context)
        {
        }

        public void WriteCommandHelp(CommandHelpContext context) =>
            context.Console.Output.Write(
                string.Join(
                    "|",
                    context.Command.Options.Select(option =>
                        $"{string.Join(",", option.Names)}:{option.IsRequired}:{option.TakesValue}")));
    }

    private sealed class ThrowCommand : ICommand<ThrowParameters>
    {
        public int Execute(CommandContext context, ThrowParameters parameters) => 0;
    }

    private sealed class TypeCommand : ICommand<TypeMetadataParameters>
    {
        public int Execute(CommandContext context, TypeMetadataParameters parameters) => 0;
    }

    private sealed class OneOrMoreCommand : ICommand<OneOrMoreParameters>
    {
        public int Execute(CommandContext context, OneOrMoreParameters parameters) => 0;
    }

    private sealed class TypeHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context)
        {
        }

        public void WriteCommandHelp(CommandHelpContext context) =>
            context.Console.Output.Write(
                string.Join(
                    ",",
                    context.Command.Arguments.Select(argument =>
                        $"{argument.DeclaredType.Name}:{argument.Arity.Minimum}:{argument.Arity.Maximum}:" +
                        $"{argument.IsRequired}:{argument.IsVariadic}")));
    }

    private sealed class ThrowHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context)
        {
        }

        public void WriteCommandHelp(CommandHelpContext context)
        {
            var command = context.Command;
            var output = context.Console.Output;

            output.WriteLine("Description:");
            output.WriteLine($"    {command.Description}");
            output.WriteLine();
            output.WriteLine("Usage:");

            var arguments = string.Join(
                " ",
                command.Arguments.Select(argument => argument.IsVariadic
                    ? $"<{argument.Name}>..."
                    : $"<{argument.Name}>"));
            output.WriteLine($"    {context.AppName} {command.Name} {arguments} [OPTIONS]");
            output.WriteLine();
            output.WriteLine("Arguments:");

            foreach (var argument in command.Arguments)
            {
                output.WriteLine($"    <{argument.Name}>  {argument.Description}");
            }

            output.WriteLine();
            output.WriteLine("Options:");

            foreach (var option in context.GlobalOptions)
            {
                var names = string.Join(", ", option.Names);
                output.WriteLine($"    {names,-36}{option.Description}");
            }
        }
    }

    private sealed class StubHelpProvider : IHelpProvider
    {
        public void WriteRootHelp(RootHelpContext context)
        {
            context.Console.Output.WriteLine("Usage:");
            context.Console.Output.WriteLine($"    {context.AppName} [OPTIONS] <COMMAND>");
            context.Console.Output.WriteLine();
            context.Console.Output.WriteLine("Options:");
            foreach (var option in context.GlobalOptions)
            {
                var names = string.Join(", ", option.Names);
                context.Console.Output.WriteLine($"    {names,-15}{option.Description}");
            }

            context.Console.Output.WriteLine();
            context.Console.Output.WriteLine("Commands:");

            var nameWidth = context.Commands.Max(command => command.Name.Length);
            foreach (var command in context.Commands)
            {
                context.Console.Output.WriteLine($"    {command.Name.PadRight(nameWidth)}  {command.Description}");
            }
        }

        public void WriteCommandHelp(CommandHelpContext context) =>
            context.Console.Output.Write($"{context.Command.Name}: {context.Command.Description}");
    }
}

public sealed partial class ThrowParameters
{
    [Argument(0, Description = "Destination for HTTP traffic")]
    public string Upstream { get; set; } = "";
}

public sealed partial class TypeMetadataParameters
{
    [Argument(0)]
    public int Count { get; set; }

    [OptionalArgument(1)]
    public int? Retries { get; set; }

    [VariadicArgument(2)]
    public string[] Tags { get; set; } = [];
}
