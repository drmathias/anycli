// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// The default enum parameter parser.
/// </summary>
/// <remarks>
/// Parsing ignores case and accepts kebab-case values by removing hyphens before matching, so
/// <c>very-slow</c> matches <c>VerySlow</c>.
/// </remarks>
public sealed class DefaultEnumParameterParser : IEnumParameterParser
{
    /// <summary>
    /// Attempts to parse an enum parameter value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="result">The parsed enum value when parsing succeeds.</param>
    /// <returns><see langword="true"/> when parsing succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse<TEnum>(
        string value,
        IFormatProvider? provider,
        out TEnum result)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse(value, ignoreCase: true, out result)) return true;

        var normalizedValue = value.Replace("-", string.Empty, StringComparison.Ordinal);
        return normalizedValue.Length != value.Length && Enum.TryParse(normalizedValue, ignoreCase: true, out result);
    }
}
