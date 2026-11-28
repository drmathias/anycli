// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides help text for a command parameter's default value.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class HelpDefaultValueAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HelpDefaultValueAttribute"/> class.
    /// </summary>
    public HelpDefaultValueAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HelpDefaultValueAttribute"/> class.
    /// </summary>
    /// <param name="text">The literal fallback display text.</param>
    public HelpDefaultValueAttribute(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Gets or sets the literal fallback display text.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the localization resource key.
    /// </summary>
    public string? ResourceKey { get; set; }
}
