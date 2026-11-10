// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace AnyCli.Benchmarks;

internal sealed class BenchmarksConfig : ManualConfig
{
    public BenchmarksConfig()
    {
        AddJob(Job.Default
            .WithWarmupCount(5)
            .WithIterationCount(15));

        AddColumn(StatisticColumn.Median);
        AddColumn(RankColumn.Arabic);
        AddDiagnoser(MemoryDiagnoser.Default);
    }
}
