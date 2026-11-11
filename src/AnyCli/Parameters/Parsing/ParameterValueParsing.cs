// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Parses raw command-line parameter values into strongly typed values.
/// </summary>
/// <remarks>
/// This type is used by generated command parameters code.
/// </remarks>
public static class ParameterValueParsing
{
    /// <summary>
    /// Parses a value using <see cref="IParsable{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="parameterName">The parameter name used in error messages.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="CommandParameterException">The value could not be parsed.</exception>
    public static T ParseParsable<T>(string value, IFormatProvider? provider, string parameterName)
        where T : IParsable<T>
    {
        return T.TryParse(value, provider, out var result) ? result : throw CreateInvalidValue(value, parameterName, typeof(T));
    }

    /// <summary>
    /// Parses a value using <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="parameterName">The parameter name used in error messages.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="CommandParameterException">The value could not be parsed.</exception>
    public static T ParseSpanParsable<T>(string value, IFormatProvider? provider, string parameterName)
        where T : ISpanParsable<T>
    {
        return T.TryParse(value.AsSpan(), provider, out var result) ? result : throw CreateInvalidValue(value, parameterName, typeof(T));
    }

    /// <summary>
    /// Parses an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="parameterName">The parameter name used in error messages.</param>
    /// <returns>The parsed enum value.</returns>
    /// <remarks>
    /// Parsing ignores case and accepts kebab-case values by removing hyphens before matching,
    /// so <c>very-slow</c> matches <c>VerySlow</c>. Numeric values are not accepted; only enum
    /// member names can be parsed.
    /// </remarks>
    /// <exception cref="CommandParameterException">The value could not be parsed.</exception>
    public static TEnum ParseEnum<TEnum>(string value, string parameterName)
        where TEnum : struct, Enum =>
        DefaultEnumParameterParser.TryParse<TEnum>(value, provider: null, out var result)
            ? result
            : throw CreateInvalidValue(value, parameterName, typeof(TEnum));

    /// <summary>
    /// Parses an enum value using a custom <see cref="IEnumParameterParser"/>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <typeparam name="TParser">The enum parser type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="parameterName">The parameter name used in error messages.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="CommandParameterException">The value could not be parsed.</exception>
    public static TEnum ParseEnumCustom<TEnum, TParser>(string value, string parameterName)
        where TEnum : struct, Enum
        where TParser : IEnumParameterParser
    {
        return TParser.TryParse<TEnum>(value, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw CreateInvalidValue(value, parameterName, typeof(TEnum));
    }

    /// <summary>
    /// Parses a value using a custom <see cref="IParameterParser{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TParser">The parser type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="parameterName">The parameter name used in error messages.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="CommandParameterException">The value could not be parsed.</exception>
    public static T ParseCustom<T, TParser>(string value, IFormatProvider? provider, string parameterName)
        where TParser : IParameterParser<T>
    {
        if (TParser.TryParse(value, provider, out var result))
        {
            return result;
        }

        throw CreateInvalidValue(value, parameterName, typeof(T));
    }

    internal static CommandParameterException CreateInvalidValue(
        string value,
        string parameterName,
        Type targetType) =>
        new($"Invalid value '{value}' for parameter '{parameterName}'. Expected {targetType.Name}.");
}
