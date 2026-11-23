// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Text.Json;

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// The default enum parameter parser.
/// </summary>
/// <remarks>
/// Parsing ignores case and accepts enum member names in their canonical kebab-case form, so
/// <c>any-cli</c> matches <c>AnyCli</c>. Malformed dash sequences are rejected. Numeric values
/// are not accepted; only enum member names can be parsed.
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

        var segments = value.Split(',', StringSplitOptions.TrimEntries);
        var names = Enum.GetNames<TEnum>();
        var policy = JsonNamingPolicy.KebabCaseLower;
        var matchedNames = new string[segments.Length];

        for (var index = 0; index < segments.Length; index++)
        {
            var segment = segments[index];
            var matchedName = Array.Find(
                names,
                name => policy.ConvertName(name).Equals(segment, StringComparison.OrdinalIgnoreCase));

            if (matchedName is null)
            {
                result = default;
                return false;
            }

            matchedNames[index] = matchedName;
        }

        return Enum.TryParse(string.Join(",", matchedNames), ignoreCase: true, out result);
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
