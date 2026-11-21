// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using XenoAtom.CommandLine;

using static AnyCli.Benchmarks.Support.BenchmarkSink;

namespace AnyCli.Benchmarks.Adapters;

internal sealed class XenoAtomCommandLineAdapter
{
    private readonly CommandApp _app;
    private readonly CommandRunConfig _runConfig = new()
    {
        Out = TextWriter.Null,
        Error = TextWriter.Null,
    };

    private XenoAtomCommandLineAdapter(CommandApp app)
    {
        _app = app;
    }

    public static XenoAtomCommandLineAdapter Create()
    {
        var input = "";
        var count = 0;
        var verbose = false;

        var app = new CommandApp("bench")
        {
            new Command("run")
            {
                { "<input>", "Input file", value => input = value },
                { "count=", "Count", (int value) => count = value },
                { "verbose", "Verbose", _ => verbose = true },
                (_, _) =>
                {
                    Accept(input, count, verbose);
                    return ValueTask.FromResult(0);
                },
            },
        };

        return new XenoAtomCommandLineAdapter(app);
    }

    public async Task<int> InvokeAsync(string[] args) =>
        await _app.RunAsync(args, _runConfig).ConfigureAwait(false);
}
