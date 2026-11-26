// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides literal help text for a parameter's default value.
/// </summary>
/// <param name="text">The literal display text.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class DefaultValueDisplayAttribute(string text) : Attribute
{
    /// <summary>
    /// Gets the literal display text.
    /// </summary>
    public string Text { get; } = text;
}
