// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

/// <summary>
/// Marks a parameters property as a variadic positional command argument.
/// </summary>
public sealed class VariadicArgumentAttribute(int position) : ArgumentAttributeBase(position)
{
    /// <summary>
    /// Gets or sets the minimum number of values required. Defaults to <c>0</c>.
    /// Set to <c>1</c> for one-or-more semantics.
    /// </summary>
    public int MinimumCount { get; set; }

    /// <summary>
    /// Gets or sets the default values used when no argument values are supplied.
    /// </summary>
    public object? DefaultValue { get; set; }
}
