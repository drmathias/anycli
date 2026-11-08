// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Parses a command-line parameter value into a custom type.
/// </summary>
/// <typeparam name="T">The parsed value type.</typeparam>
/// <remarks>
/// Specify an implementation with <see cref="ArgumentAttribute.Parser"/>,
/// <see cref="VariadicArgumentAttribute.Parser"/>, or <see cref="OptionAttribute.Parser"/>.
/// </remarks>
public interface IParameterParser<T>
{
    /// <summary>
    /// Attempts to parse a parameter value.
    /// </summary>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="result">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    static abstract bool TryParse(string value, IFormatProvider? provider, out T result);
}
