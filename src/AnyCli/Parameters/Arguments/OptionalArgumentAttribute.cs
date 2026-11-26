// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

/// <summary>
/// Marks a parameters property as an optional positional command argument.
/// </summary>
public sealed class OptionalArgumentAttribute : ArgumentAttributeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalArgumentAttribute"/> class.
    /// </summary>
    /// <param name="position">The zero-based position of the argument.</param>
    public OptionalArgumentAttribute(int position)
        : base(position)
    {
    }

    /// <summary>
    /// Gets or sets a default value provider that creates the value when the argument is not supplied.
    /// </summary>
    public Type? DefaultValueProvider { get; set; }
}
