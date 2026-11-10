// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using CliWrap;
using CliWrap.Buffered;

namespace AnyCli.Benchmarks.Support;

internal static class ProcessHost
{
    private static readonly string HostsRoot =
        Environment.GetEnvironmentVariable("BENCH_HOSTS_DIR") ??
        Path.Combine("artifacts", "benchmarks", "hosts");

    public static string JitDll(string hostName) => Path.Combine(HostsRoot, "jit", hostName, $"{hostName}.Host.dll");

    public static string AotExecutable(string hostName) =>
        Path.Combine(HostsRoot, "aot", hostName, $"{hostName}.Host{(OperatingSystem.IsWindows() ? ".exe" : "")}");

    public static Task<int> RunJitAsync(string hostName, IReadOnlyList<string> args) =>
        RunAsync("dotnet", [JitDll(hostName), .. args]);

    public static Task<int> RunAotAsync(string hostName, IReadOnlyList<string> args) =>
        RunAsync(AotExecutable(hostName), args);

    private static async Task<int> RunAsync(string target, IEnumerable<string> args)
    {
        var result = await Cli.Wrap(target)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync()
            .ConfigureAwait(false);

        return result.ExitCode;
    }
}
