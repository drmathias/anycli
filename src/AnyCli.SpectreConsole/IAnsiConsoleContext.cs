// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Provides ANSI consoles for standard output and standard error.
/// </summary>
public interface IAnsiConsoleContext
{
    /// <summary>
    /// Gets the ANSI console for standard output.
    /// </summary>
    IAnsiConsole Output { get; }

    /// <summary>
    /// Gets the ANSI console for standard error.
    /// </summary>
    IAnsiConsole Error { get; }
}
