// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Interceptors;

/// <summary>
/// Describes a raw parsed value supplied to a command argument or option.
/// </summary>
/// <remarks>
/// This is a type union that must be one of:
/// <list type="bullet">
/// <item><see cref="Flag"/></item>
/// <item><see cref="Scalar"/></item>
/// <item><see cref="Multiple"/></item>
/// </list>
/// </remarks>
#if NET11_0_OR_GREATER
public closed record ParsedValue
#else
// when no longer targeting .NET 10, refactor to a non-boxing union to avoid allocating
public abstract record ParsedValue
#endif
{
    private ParsedValue() { }

    /// <summary>
    /// A flag supplied without a value.
    /// </summary>
    public sealed record Flag : ParsedValue;

    /// <summary>
    /// A single supplied value. <see cref="Value"/> is <see langword="null"/> when an optional
    /// argument was not supplied and has no default.
    /// </summary>
    /// <param name="Value">The supplied value.</param>
    public sealed record Scalar(string? Value) : ParsedValue;

    /// <summary>
    /// A variadic argument or option with multiple supplied values.
    /// </summary>
    /// <param name="Values">The supplied values.</param>
    public sealed record Multiple(IReadOnlyList<string> Values) : ParsedValue;

    internal static ParsedValue From(object? value) => value switch
    {
        null => new Scalar(null),
        true => new Flag(),
        string scalar => new Scalar(scalar),
        IReadOnlyList<string> collection => new Multiple(collection),
        _ => throw new InvalidOperationException($"Unsupported parsed value type '{value.GetType()}'."),
    };
}
