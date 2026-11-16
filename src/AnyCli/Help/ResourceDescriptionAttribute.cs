// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Associates a command parameter with a localizable description resource key.
/// </summary>
/// <param name="resourceKey">The localization resource key.</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ResourceDescriptionAttribute(string resourceKey) : Attribute
{
    /// <summary>
    /// Gets the localization resource key.
    /// </summary>
    public string ResourceKey { get; } = resourceKey;
}
