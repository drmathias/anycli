// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Marks a parameters property as an optional command-line option.
/// </summary>
public class OptionAttribute : OptionAttributeBase
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
        : base(name, aliases)
    {
    }

    /// <summary>
    /// Gets or sets the default value used when the option is not supplied.
    /// </summary>
    public object? DefaultValue { get; set; }
}
