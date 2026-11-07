using AnyCli;

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

    private static async Task<RunResult> Run(string[] args)
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("jvln")
            .WithVersion("1.0.0")
            .AddCommand<NoOpCommand>("arm", "Starts the Jvln daemon")
            .AddCommand<NoOpCommand>("disarm", "Stops the Jvln daemon")
            .AddCommand<NoOpCommand>("status", "Retrives the current status of the Jvln daemon")
            .AddCommand<NoOpCommand>("target", "Establishes a secure connection with a Jvln server")
            .AddCommand<NoOpCommand>("throw", "Opens a Jvln tunnel")
            .AddCommand<NoOpCommand>("retrieve", "Closes a Jvln tunnel")
            .AddHelp<StubHelpProvider>()
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(args, output, error);

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private static string Normalize(string value) =>
        string.Join(
            '\n',
            value.ReplaceLineEndings("\n")
                .Split('\n')
                .Select(line => line.TrimEnd()))
            .TrimEnd('\n');

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class NoOpCommand : ICommand
    {
        public int Execute(CommandContext context) => 0;
    }

    private sealed class StubHelpProvider : IHelpProvider
    {
        public void WriteHelp(HelpContext context)
        {
            if (context.CommandName is not null)
            {
                var command = context.Commands.Single(command => command.Name == context.CommandName);
                context.Output.Write($"{command.Name}: {command.Description}");
                return;
            }

            context.Output.WriteLine("Usage:");
            context.Output.WriteLine($"    {context.AppName} [OPTIONS] <COMMAND>");
            context.Output.WriteLine();
            context.Output.WriteLine("Options:");
            foreach (var option in context.Options)
            {
                var names = string.Join(", ", option.Names);
                context.Output.WriteLine($"    {names.PadRight(15)}{option.Description}");
            }

            context.Output.WriteLine();
            context.Output.WriteLine("Commands:");

            var nameWidth = context.Commands.Max(command => command.Name.Length);
            foreach (var command in context.Commands)
            {
                context.Output.WriteLine($"    {command.Name.PadRight(nameWidth)}  {command.Description}");
            }
        }
    }
}
