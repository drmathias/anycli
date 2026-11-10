// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.CommandLine;

using static AnyCli.Benchmarks.Support.BenchmarkSink;

namespace AnyCli.Benchmarks.Adapters;

internal sealed class SystemCommandLineAdapter
{
    private readonly RootCommand _rootCommand;
    private readonly InvocationConfiguration _invocationConfiguration = new()
    {
        Output = TextWriter.Null,
        Error = TextWriter.Null,
    };

    private SystemCommandLineAdapter(RootCommand rootCommand)
    {
        _rootCommand = rootCommand;
    }

    public static SystemCommandLineAdapter Create()
    {
        var inputArgument = new Argument<string>("input");
        var countOption = new Option<int>("--count")
        {
            DefaultValueFactory = _ => 0,
        };
        var verboseOption = new Option<bool>("--verbose");

        var runCommand = new Command("run", "Runs the benchmark command");
        runCommand.Arguments.Add(inputArgument);
        runCommand.Options.Add(countOption);
        runCommand.Options.Add(verboseOption);
        runCommand.SetAction(parseResult =>
        {
            Accept(
                parseResult.GetRequiredValue(inputArgument),
                parseResult.GetValue(countOption),
                parseResult.GetValue(verboseOption));
            return 0;
        });

        var rootCommand = new RootCommand();
        rootCommand.Subcommands.Add(runCommand);

        return new SystemCommandLineAdapter(rootCommand);
    }

    public ValueTask<int> InvokeAsync(string[] args) =>
        ValueTask.FromResult(_rootCommand.Parse(args).Invoke(_invocationConfiguration));
}
