// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Benchmarks.Support;

internal static class BenchmarkArgs
{
    public static readonly string[] Typical = ["run", "input.txt", "--count", "3", "--verbose"];
}
