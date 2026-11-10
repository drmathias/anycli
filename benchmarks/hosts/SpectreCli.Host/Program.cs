// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(configurator =>
{
    configurator.ConfigureConsole(
        AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Interactive = InteractionSupport.No,
            Out = new AnsiConsoleOutput(TextWriter.Null),
        }));
    configurator.AddCommand<RunCommand>("run");
});

return await app.RunAsync(args, CancellationToken.None).ConfigureAwait(false);

internal sealed class RunCommand : Command<RunSettings>
{
    public override int Execute(
        CommandContext context,
        RunSettings settings,
        CancellationToken cancellationToken) => 0;
}

internal sealed class RunSettings : CommandSettings
{
    [CommandArgument(0, "<input>")]
    public string Input { get; init; } = "";

    [CommandOption("--count")]
    public int Count { get; init; }

    [CommandOption("--verbose")]
    public bool Verbose { get; init; }
}
