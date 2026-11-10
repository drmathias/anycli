// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Benchmarks.Support;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace AnyCli.Benchmarks;

[Config(typeof(BenchmarksConfig))]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ColdStartAotBenchmarks
{
    private static readonly string[] Args = BenchmarkArgs.Typical;

    /// <remarks>
    /// Spectre.Console.Cli does not support NativeAOT, so it is excluded from AOT benchmarks.
    /// </remarks>
    [GlobalSetup]
    public async Task Setup()
    {
        Verify(await ProcessHost.RunAotAsync("AnyCli", Args).ConfigureAwait(false), "AnyCli");
        Verify(await ProcessHost.RunAotAsync("SystemCommandLine", Args).ConfigureAwait(false), "System.CommandLine");
        Verify(await ProcessHost.RunAotAsync("ConsoleAppFramework", Args).ConfigureAwait(false), "ConsoleAppFramework");
    }

    [Benchmark(Baseline = true)]
    public Task<int> AnyCli() => ProcessHost.RunAotAsync("AnyCli", Args);

    [Benchmark]
    public Task<int> SystemCommandLine() => ProcessHost.RunAotAsync("SystemCommandLine", Args);

    [Benchmark]
    public Task<int> ConsoleAppFramework() => ProcessHost.RunAotAsync("ConsoleAppFramework", Args);

    private static void Verify(int exitCode, string framework)
    {
        if (exitCode != 0) throw new InvalidOperationException($"{framework} AOT host failed with exit code {exitCode}.");
    }
}
