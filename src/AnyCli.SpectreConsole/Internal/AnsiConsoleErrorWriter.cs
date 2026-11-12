// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

internal sealed class AnsiConsoleErrorWriter(IAnsiConsole error) : IAnsiConsoleErrorWriter
{
    public IAnsiConsole Error { get; } = error;
}
