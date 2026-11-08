// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Marks a parameters property as a command-line option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class OptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttribute"/> class with an
    /// automatically generated long name.
    /// </summary>
    public OptionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionAttribute"/> class.
    /// </summary>
    /// <param name="name">The primary option name, such as <c>--output</c> or <c>-o</c>.</param>
    /// <param name="aliases">Additional option names.</param>
    public OptionAttribute(string name, params string[] aliases)
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
    /// Gets or sets whether the option must be supplied.
    /// </summary>
    public bool Required { get; set; }

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
