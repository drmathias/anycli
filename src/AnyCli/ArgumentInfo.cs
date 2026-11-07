// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a positional command argument.
/// </summary>
/// <param name="Name">The argument name.</param>
/// <param name="Description">A short description of the argument.</param>
/// <param name="ArgumentType">The declared argument type.</param>
/// <param name="Arity">The number of values the argument accepts.</param>
public sealed record ArgumentInfo(
    string Name,
    string Description,
    Type ArgumentType,
    ArgumentArity Arity)
{
    /// <summary>
    /// Gets a value indicating whether the argument is required.
    /// </summary>
    public bool IsRequired => Arity.IsRequired;

    /// <summary>
    /// Gets a value indicating whether the argument accepts more than one value.
    /// </summary>
    public bool IsVariadic => Arity.IsVariadic;
}
