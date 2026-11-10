// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using ConsoleAppFramework;

using static AnyCli.Benchmarks.Support.BenchmarkSink;

namespace AnyCli.Benchmarks.Adapters;

internal sealed class ConsoleAppFrameworkAdapter
{
    private readonly Action<string[]> _run;

    private ConsoleAppFrameworkAdapter(Action<string[]> run)
    {
        _run = run;
    }

    public static ConsoleAppFrameworkAdapter Create()
    {
        var app = ConsoleApp.Create();
        app.Add("run", ([Argument] string input, int count = 0, bool verbose = false) => Accept(input, count, verbose));
        return new ConsoleAppFrameworkAdapter(app.Run);
    }

    public ValueTask<int> InvokeAsync(string[] args)
    {
        _run.Invoke(args);
        return ValueTask.FromResult(0);
    }
}
