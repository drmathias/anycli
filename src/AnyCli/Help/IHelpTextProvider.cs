// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

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
    /// Gets a localized description for a command, option, or argument.
    /// </summary>
    /// <param name="context">The description context.</param>
    /// <returns>The localized description.</returns>
    string GetDescription(HelpDescriptionContext context);
}
