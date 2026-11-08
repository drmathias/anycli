// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Console;

/// <summary>
/// Provides the standard input, output, and error streams for a command.
/// </summary>
public interface IConsole : IConsoleWriter
{
    /// <summary>
    /// Gets the reader used for standard input.
    /// </summary>
    IInputReader Input { get; }
}
