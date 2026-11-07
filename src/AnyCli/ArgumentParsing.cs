// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli;

/// <summary>
/// Parses raw command-line argument values into strongly typed values.
/// </summary>
/// <remarks>
/// This type is used by generated command settings code.
/// </remarks>
public static class ArgumentParsing
{
    /// <summary>
    /// Parses a value using <see cref="IParsable{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The raw argument value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="argumentName">The argument name used in error messages.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="CommandArgumentException">The value could not be parsed.</exception>
    public static T ParseParsable<T>(string value, IFormatProvider? provider, string argumentName)
        where T : IParsable<T>
    {
        return T.TryParse(value, provider, out var result) ? result : throw CreateInvalidValue(value, argumentName, typeof(T));
    }

    /// <summary>
    /// Parses a value using <see cref="ISpanParsable{TSelf}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The raw argument value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="argumentName">The argument name used in error messages.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="CommandArgumentException">The value could not be parsed.</exception>
    public static T ParseSpanParsable<T>(string value, IFormatProvider? provider, string argumentName)
        where T : ISpanParsable<T>
    {
        return T.TryParse(value.AsSpan(), provider, out var result) ? result : throw CreateInvalidValue(value, argumentName, typeof(T));
    }

    /// <summary>
    /// Parses an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The raw argument value.</param>
    /// <param name="argumentName">The argument name used in error messages.</param>
    /// <returns>The parsed enum value.</returns>
    /// <remarks>
    /// Parsing ignores case and accepts kebab-case values by removing hyphens before matching,
    /// so <c>very-slow</c> matches <c>VerySlow</c>.
    /// </remarks>
    /// <exception cref="CommandArgumentException">The value could not be parsed.</exception>
    public static TEnum ParseEnum<TEnum>(string value, string argumentName)
        where TEnum : struct, Enum =>
        DefaultEnumArgumentParser.TryParse<TEnum>(value, provider: null, out var result)
            ? result
            : throw CreateInvalidValue(value, argumentName, typeof(TEnum));

    /// <summary>
    /// Parses an enum value using a custom <see cref="IEnumArgumentParser"/>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <typeparam name="TParser">The enum parser type.</typeparam>
    /// <param name="value">The raw argument value.</param>
    /// <param name="argumentName">The argument name used in error messages.</param>
    /// <returns>The parsed enum value.</returns>
    /// <exception cref="CommandArgumentException">The value could not be parsed.</exception>
    public static TEnum ParseEnumCustom<TEnum, TParser>(string value, string argumentName)
        where TEnum : struct, Enum
        where TParser : IEnumArgumentParser
    {
        return TParser.TryParse<TEnum>(value, CultureInfo.InvariantCulture, out var result)
            ? result
            : throw CreateInvalidValue(value, argumentName, typeof(TEnum));
    }

    /// <summary>
    /// Parses a value using a custom <see cref="IArgumentParser{T}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TParser">The parser type.</typeparam>
    /// <param name="value">The raw argument value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="argumentName">The argument name used in error messages.</param>
    /// <returns>The parsed value.</returns>
    /// <exception cref="CommandArgumentException">The value could not be parsed.</exception>
    public static T ParseCustom<T, TParser>(string value, IFormatProvider? provider, string argumentName)
        where TParser : IArgumentParser<T>
    {
        if (TParser.TryParse(value, provider, out var result))
        {
            return result;
        }

        throw CreateInvalidValue(value, argumentName, typeof(T));
    }

    internal static CommandArgumentException CreateInvalidValue(
        string value,
        string argumentName,
        Type targetType) =>
        new($"Invalid value '{value}' for argument '{argumentName}'. Expected {targetType.Name}.");
}
