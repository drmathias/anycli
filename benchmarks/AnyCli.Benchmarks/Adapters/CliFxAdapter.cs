// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

using static AnyCli.Benchmarks.Support.BenchmarkSink;

namespace AnyCli.Benchmarks.Adapters;

internal sealed class CliFxAdapter
{
    private static readonly IReadOnlyDictionary<string, string> EmptyEnvironment =
        new Dictionary<string, string>();

    private readonly CommandLineApplication _app;

    private CliFxAdapter(CommandLineApplication app)
    {
        _app = app;
    }

    public static CliFxAdapter Create()
    {
        var app = new CommandLineApplicationBuilder()
            .AddCommand(CliFxRunCommand.Descriptor)
            .UseConsole(new FakeConsole())
            .Build();

        return new CliFxAdapter(app);
    }

    public async Task<int> InvokeAsync(string[] args) =>
        await _app.RunAsync(args, EmptyEnvironment).ConfigureAwait(false);
}

[Command("run")]
internal sealed partial class CliFxRunCommand : ICommand
{
    [CommandParameter(0)]
    public required string Input { get; set; }

    [CommandOption("count")]
    public int Count { get; set; }

    [CommandOption("verbose")]
    public bool Verbose { get; set; }

    public ValueTask ExecuteAsync(IConsole console)
    {
        Accept(Input, Count, Verbose);
        return default;
    }
}
