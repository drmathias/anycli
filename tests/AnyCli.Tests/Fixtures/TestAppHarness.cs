// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests.Fixtures;

internal static class TestAppHarness
{
    public static Task<TestAppResult> RunAsync(ConsoleApp app, params string[] args) =>
        RunAsync(app, args, new StringReader(string.Empty));

    public static async Task<TestAppResult> RunAsync(ConsoleApp app, string[] args, TextReader input)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(args, new TextConsole(input, output, error));

        return new TestAppResult(exitCode, output.ToString(), error.ToString());
    }
}
