// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Asynchronously parses command-line parameter values for enum types.
/// </summary>
/// <remarks>
/// Register an implementation with <see cref="EnumParameterParserAttribute{TParser}"/> to override
/// the default enum parsing behavior for the application. Implementations are resolved from the
/// configured service provider, or created with a public parameterless constructor.
/// </remarks>
public interface IAsyncEnumParameterParser
{
    /// <summary>
    /// Attempts to parse an enum parameter value asynchronously.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The raw parameter value.</param>
    /// <param name="provider">The culture used to parse the value.</param>
    /// <param name="cancellationToken">The cancellation token for the current invocation.</param>
    /// <returns>The parse result.</returns>
    ValueTask<ParameterParseResult<TEnum>> TryParseAsync<TEnum>(
        string value,
        IFormatProvider? provider,
        CancellationToken cancellationToken)
        where TEnum : struct, Enum;
}
