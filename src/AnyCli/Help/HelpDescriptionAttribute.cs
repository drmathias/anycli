// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides help text for a command parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class HelpDescriptionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HelpDescriptionAttribute"/> class.
    /// </summary>
    public HelpDescriptionAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpDescriptionAttribute"/> class.
    /// </summary>
    /// <param name="text">The literal fallback help text.</param>
    public HelpDescriptionAttribute(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets or sets the literal fallback help text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the localization resource key.
    /// </summary>
    public string? ResourceKey { get; set; }
}
