// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Parses command-line argument values for enum types.
/// </summary>
/// <remarks>
/// Register an implementation with
/// <see cref="CommandParsingDefaults.SetEnumArgumentParser{TParser}"/> to override the default
/// enum parsing behavior for the application.
/// </remarks>
public interface IEnumArgumentParser
{
    /// <summary>
    /// Attempts to parse an enum argument value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The raw argument value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="result">The parsed enum value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    static abstract bool TryParse<TEnum>(
        string value,
        IFormatProvider? provider,
        out TEnum result)
        where TEnum : struct, Enum;
}
