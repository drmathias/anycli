// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

/// <summary>
/// Describes a positional command argument.
/// </summary>
/// <param name="Name">The argument name.</param>
/// <param name="Description">A short description of the argument.</param>
/// <param name="DeclaredType">The declared argument type.</param>
/// <param name="Arity">The number of values the argument accepts.</param>
public sealed record ArgumentInfo(
    string Name,
    HelpText Description,
    Type DeclaredType,
    ArgumentArity Arity)
{
    /// <summary>
    /// Gets a value indicating whether the argument is required.
    /// </summary>
    /// <remarks>
    /// A variadic argument is optional when a default value or property initializer is available,
    /// even when its minimum count is greater than zero.
    /// </remarks>
    public bool IsRequired => Arity.Minimum > 0 && DefaultValue is null;

    /// <summary>
    /// Gets the configured default value source, when one is defined.
    /// </summary>
    public DefaultValueInfo? DefaultValue { get; init; }
}
