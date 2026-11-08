// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

/// <summary>
/// Base class for attributes that mark a property as a positional command argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public abstract class ArgumentAttributeBase : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttributeBase"/> class.
    /// </summary>
    /// <param name="position">The zero-based position of the argument.</param>
    protected ArgumentAttributeBase(int position)
    {
        Position = position;
    }

    /// <summary>
    /// Gets the zero-based position of the argument.
    /// </summary>
    public int Position { get; }

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
    /// <see cref="IParameterParser{T}"/> for the argument value type.
    /// </summary>
    public Type? Parser { get; set; }
}
