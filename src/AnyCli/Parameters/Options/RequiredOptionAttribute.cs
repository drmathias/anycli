// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

/// <summary>
/// Marks a parameters property as a required command-line option.
/// </summary>
public sealed class RequiredOptionAttribute : OptionAttributeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredOptionAttribute"/> class with an
    /// automatically generated long name.
    /// </summary>
    public RequiredOptionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredOptionAttribute"/> class.
    /// </summary>
    /// <param name="name">The primary option name, such as <c>--output</c> or <c>-o</c>.</param>
    /// <param name="aliases">Additional option names.</param>
    public RequiredOptionAttribute(string name, params string[] aliases)
        : base(name, aliases)
    {
    }
}
