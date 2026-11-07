// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Marks a settings property as a positional command argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ArgumentAttribute(int position) : Attribute
{
    /// <summary>
    /// Gets the zero-based position of the argument.
    /// </summary>
    public int Position { get; } = position;

    /// <summary>
    /// Gets or sets whether the argument is optional.
    /// </summary>
    public bool Optional { get; set; }

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
    /// <see cref="IArgumentParser{T}"/> for the argument value type.
    /// </summary>
    public Type? Parser { get; set; }
}
