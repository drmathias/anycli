// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Configures command-line error output rendered by Spectre.Console.
/// </summary>
public sealed record SpectreErrorOptions
{
    /// <summary>
    /// Gets the style used for error messages.
    /// </summary>
    public Style ErrorStyle { get; init; } = new(foreground: Color.Red);

    /// <summary>
    /// Gets the console options to use, or <see langword="null"/> to use the context console.
    /// </summary>
    public SpectreConsoleOptions? ConsoleOptions { get; init; }
}
