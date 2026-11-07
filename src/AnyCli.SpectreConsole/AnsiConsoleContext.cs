// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

internal sealed class AnsiConsoleContext(IAnsiConsole output, IAnsiConsole error) : IAnsiConsoleContext
{
    public IAnsiConsole Output { get; } = output;

    public IAnsiConsole Error { get; } = error;
}
