// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Help;

/// <summary>
/// Formats default values for help output.
/// </summary>
public interface IHelpValueFormatter
{
    /// <summary>
    /// Formats a default value.
    /// </summary>
    /// <param name="value">The default value.</param>
    /// <param name="culture">The culture used to format the value.</param>
    /// <returns>
    /// The formatted value, or <see langword="null"/> when the default should not be displayed.
    /// </returns>
    string? Format(object? value, CultureInfo culture);
}
