// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters;

/// <summary>
/// Describes the configured source of a parameter's default value.
/// </summary>
public abstract record DefaultValueInfo
{
    /// <summary>
    /// Gets the localized display text for the default value.
    /// </summary>
    public HelpText DisplayText { get; init; }

    /// <summary>
    /// Describes a default supplied by a property initializer.
    /// </summary>
    /// <param name="Value">
    /// The captured initializer value, or <see langword="null"/> when the initializer is computed.
    /// </param>
    public sealed record Initializer(object? Value) : DefaultValueInfo;

    /// <summary>
    /// Describes a default supplied by a default value provider.
    /// </summary>
    /// <param name="ProviderType">The provider type.</param>
    public sealed record Provider(Type ProviderType) : DefaultValueInfo;
}
