// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Benchmarks.Support;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace AnyCli.Benchmarks;

[Config(typeof(BenchmarksConfig))]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ColdStartJitBenchmarks
{
    private static readonly string[] Args = BenchmarkArgs.Typical;

    [GlobalSetup]
    public async Task Setup()
    {
        Verify(await ProcessHost.RunJitAsync("AnyCli", Args).ConfigureAwait(false), "AnyCli");
        Verify(await ProcessHost.RunJitAsync("SystemCommandLine", Args).ConfigureAwait(false), "System.CommandLine");
        Verify(await ProcessHost.RunJitAsync("SpectreCli", Args).ConfigureAwait(false), "Spectre.Console.Cli");
        Verify(await ProcessHost.RunJitAsync("ConsoleAppFramework", Args).ConfigureAwait(false), "ConsoleAppFramework");
    }

    [Benchmark(Baseline = true)]
    public Task<int> AnyCli() => ProcessHost.RunJitAsync("AnyCli", Args);

    [Benchmark]
    public Task<int> SystemCommandLine() => ProcessHost.RunJitAsync("SystemCommandLine", Args);

    [Benchmark]
    public Task<int> SpectreConsoleCli() => ProcessHost.RunJitAsync("SpectreCli", Args);

    [Benchmark]
    public Task<int> ConsoleAppFramework() => ProcessHost.RunJitAsync("ConsoleAppFramework", Args);

    private static void Verify(int exitCode, string framework)
    {
        if (exitCode != 0) throw new InvalidOperationException($"{framework} cold-start host failed with exit code {exitCode}.");
    }
}
