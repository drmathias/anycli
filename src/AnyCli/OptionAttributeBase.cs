// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Base class for attributes that mark a property as a command-line option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public abstract class OptionAttributeBase : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttributeBase"/> class with an
    /// automatically generated long name.
    /// </summary>
    protected OptionAttributeBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttributeBase"/> class.
    /// </summary>
    /// <param name="name">The primary option name, such as <c>--output</c> or <c>-o</c>.</param>
    /// <param name="aliases">Additional option names.</param>
    protected OptionAttributeBase(string name, params string[] aliases)
    {
        Name = name;
        Aliases = aliases;
    }

    /// <summary>
    /// Gets the primary option name, or <see langword="null"/> to generate one from the
    /// property name.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the additional option names.
    /// </summary>
    public string[] Aliases { get; } = [];

    /// <summary>
    /// Gets or sets the description shown in help output.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a custom parser type implementing
    /// <see cref="IParameterParser{T}"/> for the option value type.
    /// </summary>
    public Type? Parser { get; set; }
}
