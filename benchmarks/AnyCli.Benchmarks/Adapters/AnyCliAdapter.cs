// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Commands.Execution;
using AnyCli.Commands.Registration;
using AnyCli.Console;
using AnyCli.Parameters.Arguments;
using AnyCli.Parameters.Options;

using static AnyCli.Benchmarks.Support.BenchmarkSink;

namespace AnyCli.Benchmarks.Adapters;

internal sealed class AnyCliAdapter
{
    private readonly ConsoleApp _app;
    private readonly IConsole _console;

    private AnyCliAdapter(ConsoleApp app, IConsole console)
    {
        _app = app;
        _console = console;
    }

    public static AnyCliAdapter Create()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("bench")
            .ConfigureCommands(commands =>
                commands.AddCommand<RunCommand, RunParameters>("run"))
            .Build();

        return new AnyCliAdapter(
            app,
            new TextConsole(TextReader.Null, TextWriter.Null, TextWriter.Null));
    }

    public async Task<int> InvokeAsync(string[] args) => await _app.RunAsync(args, _console).ConfigureAwait(false);

    private sealed class RunCommand : ICommand<RunParameters>
    {
        public int Execute(CommandContext context, RunParameters parameters)
        {
            Accept(parameters.Input, parameters.Count, parameters.Verbose);
            return 0;
        }
    }
}

public sealed partial class RunParameters
{
    [Argument(0)]
    public string Input { get; set; } = "";

    [Option("--count")]
    public int Count { get; set; }

    [Option("--verbose")]
    public bool Verbose { get; set; }
}
