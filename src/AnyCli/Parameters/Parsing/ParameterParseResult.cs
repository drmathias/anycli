// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Represents the result of an asynchronous parameter parse attempt.
/// </summary>
/// <typeparam name="T">The parsed type.</typeparam>
public readonly struct ParameterParseResult<T>
{
    private ParameterParseResult(bool success, T? value)
    {
        Success = success;
        Value = value;
    }

    /// <summary>
    /// Gets a value indicating whether parsing succeeded.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the parsed value when <see cref="Success"/> is <see langword="true"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The parsed value.</param>
    /// <returns>A successful parse result.</returns>
    public static ParameterParseResult<T> Succeeded(T value) => new(true, value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static ParameterParseResult<T> Failed => default;
}
