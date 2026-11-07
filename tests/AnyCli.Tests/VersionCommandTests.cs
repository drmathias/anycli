// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class VersionCommandTests
{
    [Test]
    [Arguments("--version")]
    [Arguments("-v")]
    public async Task VersionFlag_WritesConfiguredVersionToOutput(string flag)
    {
        var result = await Run(flag, version: "1.2.3");

        await Assert.That(result.Output).IsEqualTo($"1.2.3{Environment.NewLine}");
    }

    [Test]
    [Arguments("--version")]
    [Arguments("-v")]
    public async Task VersionFlag_ReturnsZeroExitCode(string flag)
    {
        var result = await Run(flag);

        await Assert.That(result.ExitCode).IsEqualTo(0);
    }

    [Test]
    [Arguments("--version")]
    [Arguments("-v")]
    public async Task VersionFlag_WritesNothingToError(string flag)
    {
        var result = await Run(flag);

        await Assert.That(result.Error).IsEmpty();
    }

    private static async Task<RunResult> Run(string flag, string version = "1.2.3")
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion(version)
            .Build();

        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync([flag], output, error);

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);
}
