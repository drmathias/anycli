// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

internal sealed class AnsiConsoleWriter(IAnsiConsole output, IAnsiConsole error) : IAnsiConsoleWriter
{
    public IAnsiConsole Output { get; } = output;

    public IAnsiConsole Error { get; } = error;
}
