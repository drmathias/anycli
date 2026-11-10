// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Benchmarks.Support;

internal static class BenchmarkSink
{
    private static int _value;

    public static int Value => Volatile.Read(ref _value);

    public static int Expected(string input, int count, bool verbose) =>
        input.Length + count + (verbose ? 1 : 0);

    public static void Accept(string input, int count, bool verbose) =>
        Volatile.Write(ref _value, Expected(input, count, verbose));

    public static void Reset() => Volatile.Write(ref _value, 0);
}
