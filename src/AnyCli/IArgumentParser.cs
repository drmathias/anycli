// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Parses a command-line argument value into a custom type.
/// </summary>
/// <typeparam name="T">The parsed value type.</typeparam>
/// <remarks>
/// Specify an implementation with <see cref="ArgumentAttribute.Parser"/> or
/// <see cref="VariadicArgumentAttribute.Parser"/>.
/// </remarks>
public interface IArgumentParser<T>
{
    /// <summary>
    /// Attempts to parse an argument value.
    /// </summary>
    /// <param name="value">The raw argument value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="result">The parsed value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    static abstract bool TryParse(string value, IFormatProvider? provider, out T result);
}
