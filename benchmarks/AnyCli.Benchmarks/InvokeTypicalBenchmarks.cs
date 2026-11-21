// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Benchmarks.Adapters;
using AnyCli.Benchmarks.Support;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace AnyCli.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(BenchmarksConfig))]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class InvokeTypicalBenchmarks
{
    private static readonly string[] Args = BenchmarkArgs.Typical;

    private AnyCliAdapter _anyCli = null!;
    private SystemCommandLineAdapter _systemCommandLine = null!;
    private SpectreCliAdapter _spectreCli = null!;
    private ConsoleAppFrameworkAdapter _consoleAppFramework = null!;
    private CliFxAdapter _cliFx = null!;
    private XenoAtomCommandLineAdapter _xenoAtom = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _anyCli = AnyCliAdapter.Create();
        _systemCommandLine = SystemCommandLineAdapter.Create();
        _spectreCli = SpectreCliAdapter.Create();
        _consoleAppFramework = ConsoleAppFrameworkAdapter.Create();
        _cliFx = CliFxAdapter.Create();
        _xenoAtom = XenoAtomCommandLineAdapter.Create();

        Verify(await _anyCli.InvokeAsync(Args).ConfigureAwait(false), "AnyCli");
        Verify(await _systemCommandLine.InvokeAsync(Args).ConfigureAwait(false), "System.CommandLine");
        Verify(await _spectreCli.InvokeAsync(Args).ConfigureAwait(false), "Spectre.Console.Cli");
        Verify(await _consoleAppFramework.InvokeAsync(Args).ConfigureAwait(false), "ConsoleAppFramework");
        Verify(await _cliFx.InvokeAsync(Args).ConfigureAwait(false), "CliFx");
        Verify(await _xenoAtom.InvokeAsync(Args).ConfigureAwait(false), "XenoAtom.CommandLine");
    }

    [Benchmark(Baseline = true)]
    public Task<int> AnyCli() => _anyCli.InvokeAsync(Args);

    [Benchmark]
    public ValueTask<int> SystemCommandLine() => _systemCommandLine.InvokeAsync(Args);

    [Benchmark]
    public Task<int> SpectreConsoleCli() => _spectreCli.InvokeAsync(Args);

    [Benchmark]
    public ValueTask<int> ConsoleAppFramework() => _consoleAppFramework.InvokeAsync(Args);

    [Benchmark]
    public Task<int> CliFx() => _cliFx.InvokeAsync(Args);

    [Benchmark]
    public Task<int> XenoAtomCommandLine() => _xenoAtom.InvokeAsync(Args);

    private static void Verify(int exitCode, string framework)
    {
        var expected = BenchmarkSink.Expected("input.txt", 3, true);
        if (exitCode != 0 || BenchmarkSink.Value != expected)
        {
            throw new InvalidOperationException(
                $"{framework} did not execute the benchmark command correctly. " +
                $"Exit code: {exitCode}, sink: {BenchmarkSink.Value}, expected: {expected}.");
        }
    }
}
