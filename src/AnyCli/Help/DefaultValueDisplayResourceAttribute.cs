// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Associates a parameter's default value with a localizable display text resource key.
/// </summary>
/// <param name="resourceKey">The localization resource key.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class DefaultValueDisplayResourceAttribute(string resourceKey) : Attribute
{
    /// <summary>
    /// Gets the localization resource key.
    /// </summary>
    public string ResourceKey { get; } = resourceKey;
}
