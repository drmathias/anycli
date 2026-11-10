// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;
using Spectre.Console.Cli;

using static AnyCli.Benchmarks.Support.BenchmarkSink;

namespace AnyCli.Benchmarks.Adapters;

internal sealed class SpectreCliAdapter
{
    private readonly CommandApp _app;

    private SpectreCliAdapter(CommandApp app)
    {
        _app = app;
    }

    public static SpectreCliAdapter Create()
    {
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

        return new SpectreCliAdapter(app);
    }

    public async Task<int> InvokeAsync(string[] args) =>
        await _app.RunAsync(args, CancellationToken.None).ConfigureAwait(false);

    private sealed class RunCommand : Command<RunSettings>
    {
        public override int Execute(
            CommandContext context,
            RunSettings settings,
            CancellationToken cancellationToken)
        {
            Accept(settings.Input, settings.Count, settings.Verbose);
            return 0;
        }
    }

    private sealed class RunSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        public string Input { get; init; } = "";

        [CommandOption("--count")]
        public int Count { get; init; }

        [CommandOption("--verbose")]
        public bool Verbose { get; init; }
    }
}
