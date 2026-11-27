// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Asynchronously parses a command-line parameter value into a custom type.
/// </summary>
/// <typeparam name="T">The parsed type.</typeparam>
/// <remarks>
/// Specify an implementation with <see cref="ArgumentAttributeBase.Parser"/> or
/// <see cref="OptionAttributeBase.Parser"/>, or register it with
/// <see cref="ParameterValueParserAttribute{TValue, TParser}"/>. Implementations are resolved
/// from the configured service provider, or created with a public parameterless constructor.
/// </remarks>
public interface IAsyncParameterParser<T>
{
    /// <summary>
    /// Attempts to parse a parameter value asynchronously.
    /// </summary>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="cancellationToken">The cancellation token for the current invocation.</param>
    /// <returns>The parse result.</returns>
    ValueTask<ParameterParseResult<T>> TryParseAsync(
        string value,
        IFormatProvider? provider,
        CancellationToken cancellationToken);
}
