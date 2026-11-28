// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Configures the styles used by <see cref="SpectreHelpRenderer"/>.
/// </summary>
public sealed record SpectreHelpOptions
{
    /// <summary>
    /// Gets the style used for section headings.
    /// </summary>
    public Style HeadingStyle { get; init; } = new(foreground: Color.Aqua);

    /// <summary>
    /// Gets the style used for usage lines.
    /// </summary>
    public Style UsageStyle { get; init; } = Style.Plain;

    /// <summary>
    /// Gets the style used for the options placeholder in usage lines.
    /// </summary>
    public Style OptionsPlaceholderStyle { get; init; } = new(foreground: Color.Grey);

    /// <summary>
    /// Gets the style used for the command placeholder in usage lines.
    /// </summary>
    public Style CommandPlaceholderStyle { get; init; } = new(foreground: Color.White);

    /// <summary>
    /// Gets the style used for command names.
    /// </summary>
    public Style CommandStyle { get; init; } = Style.Plain;

    /// <summary>
    /// Gets the style used for option syntax.
    /// </summary>
    public Style OptionStyle { get; init; } = Style.Plain;

    /// <summary>
    /// Gets the style used for argument syntax.
    /// </summary>
    public Style ArgumentStyle { get; init; } = new(foreground: Color.MediumPurple1);

    /// <summary>
    /// Gets the style used for descriptions.
    /// </summary>
    public Style DescriptionStyle { get; init; } = Style.Plain;
}
