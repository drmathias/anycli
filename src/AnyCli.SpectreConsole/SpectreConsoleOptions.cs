// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Configures how an <see cref="IAnsiConsole"/> is created from an output writer.
/// </summary>
public sealed record SpectreConsoleOptions
{
    /// <summary>
    /// Gets the ANSI support mode.
    /// </summary>
    public AnsiSupport Ansi { get; init; } = AnsiSupport.Detect;

    /// <summary>
    /// Gets the colour system to use.
    /// </summary>
    public ColorSystemSupport ColorSystem { get; init; } = ColorSystemSupport.Detect;

    /// <summary>
    /// Gets the interaction mode.
    /// </summary>
    public InteractionSupport Interactive { get; init; } = InteractionSupport.No;

    /// <summary>
    /// Gets options that force deterministic ANSI output for tests.
    /// </summary>
    public static SpectreConsoleOptions ForTesting { get; } = new()
    {
        Ansi = AnsiSupport.Yes,
        ColorSystem = ColorSystemSupport.Standard,
        Interactive = InteractionSupport.No,
    };
}
