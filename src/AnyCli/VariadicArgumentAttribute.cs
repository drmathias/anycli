// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Marks a parameters property as a variadic positional command argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class VariadicArgumentAttribute(int position) : Attribute
{
    /// <summary>
    /// Gets the zero-based position of the argument.
    /// </summary>
    public int Position { get; } = position;

    /// <summary>
    /// Gets or sets the minimum number of values required. Defaults to <c>0</c>.
    /// Set to <c>1</c> for one-or-more semantics.
    /// </summary>
    public int MinimumCount { get; set; }

    /// <summary>
    /// Gets or sets the argument name. Defaults to the property name when not set.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the argument description shown in help output.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a custom parser type implementing
    /// <see cref="IParameterParser{T}"/> for the array element type.
    /// </summary>
    public Type? Parser { get; set; }
}
