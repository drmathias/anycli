// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// The default enum parameter parser.
/// </summary>
/// <remarks>
/// Parsing ignores case and accepts kebab-case values by removing hyphens before matching, so
/// <c>very-slow</c> matches <c>VerySlow</c>. Numeric values are not accepted; only enum member
/// names can be parsed.
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
        if (ContainsNumericValue(value))
        {
            result = default;
            return false;
        }

        if (Enum.TryParse(value, ignoreCase: true, out result)) return true;

        var normalizedValue = value.Replace("-", string.Empty, StringComparison.Ordinal);
        return normalizedValue.Length != value.Length &&
               !ContainsNumericValue(normalizedValue) &&
               Enum.TryParse(normalizedValue, ignoreCase: true, out result);
    }

    private static bool ContainsNumericValue(string value)
    {
        foreach (var segment in value.Split(','))
        {
            if (IsNumericValue(segment.AsSpan().Trim())) return true;
        }
        return false;
    }

    private static bool IsNumericValue(ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return false;

        if (value[0] is '-' or '+')
        {
            value = value[1..];
            if (value.Length == 0) return false;
        }

        if (value.Length > 2 && value[0] == '0' && value[1] is 'x' or 'X')
        {
            foreach (var character in value[2..])
            {
                if (!IsHexDigit(character)) return false;
            }

            return true;
        }

        foreach (var character in value)
        {
            if (!char.IsDigit(character)) return false;
        }

        return true;
    }

    private static bool IsHexDigit(char value) =>
        value is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');
}
