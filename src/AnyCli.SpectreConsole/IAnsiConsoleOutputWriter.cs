// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Provides the ANSI console for standard output.
/// </summary>
public interface IAnsiConsoleOutputWriter
{
    /// <summary>
    /// Gets the ANSI console for standard output.
    /// </summary>
    IAnsiConsole Output { get; }
}
