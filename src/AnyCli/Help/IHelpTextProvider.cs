// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Help;

/// <summary>
/// Provides localized text for help output.
/// </summary>
public interface IHelpTextProvider
{
    /// <summary>
    /// Gets the usage section heading.
    /// </summary>
    /// <returns>The usage heading.</returns>
    string GetUsageHeading();

    /// <summary>
    /// Gets the description section heading.
    /// </summary>
    /// <returns>The description heading.</returns>
    string GetDescriptionHeading();

    /// <summary>
    /// Gets the commands section heading.
    /// </summary>
    /// <returns>The commands heading.</returns>
    string GetCommandsHeading();

    /// <summary>
    /// Gets the arguments section heading.
    /// </summary>
    /// <returns>The arguments heading.</returns>
    string GetArgumentsHeading();

    /// <summary>
    /// Gets the options section heading.
    /// </summary>
    /// <returns>The options heading.</returns>
    string GetOptionsHeading();

    /// <summary>
    /// Gets the localized options placeholder used in usage syntax.
    /// </summary>
    /// <returns>The options placeholder.</returns>
    string GetOptionsPlaceholder();

    /// <summary>
    /// Gets the localized command placeholder used in usage syntax.
    /// </summary>
    /// <returns>The command placeholder.</returns>
    string GetCommandPlaceholder();

    /// <summary>
    /// Gets a localized description for a command, option, or argument.
    /// </summary>
    /// <param name="context">The description context.</param>
    /// <returns>The localized description.</returns>
    string GetDescription(HelpDescriptionContext context);

    /// <summary>
    /// Gets the localized label used before a parameter's default value.
    /// </summary>
    /// <returns>The localized default value label.</returns>
    string GetDefaultValueLabel();

    /// <summary>
    /// Gets the localized display text for a parameter's default value.
    /// </summary>
    /// <param name="text">The configured default value display text.</param>
    /// <returns>The localized display text.</returns>
    string GetDefaultValueText(HelpText text);

    /// <summary>
    /// Gets the culture used to format values in help text.
    /// </summary>
    /// <returns>The formatting culture.</returns>
    CultureInfo GetFormatCulture();
}
