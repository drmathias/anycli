// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Describes help text using literal text, a resource key, or both.
/// </summary>
/// <param name="Text">The literal fallback text.</param>
/// <param name="ResourceKey">The localization resource key.</param>
public readonly record struct HelpText(
    string? Text = null,
    string? ResourceKey = null)
{
    /// <summary>
    /// Gets empty help text.
    /// </summary>
    public static HelpText Empty => default;

    /// <summary>
    /// Creates help text that resolves a localization resource key.
    /// </summary>
    /// <param name="resourceKey">The localization resource key.</param>
    /// <returns>The help text.</returns>
    public static HelpText FromResource(string resourceKey) =>
        new(ResourceKey: resourceKey);

    /// <summary>
    /// Gets a value indicating whether the help text is empty.
    /// </summary>
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Text) &&
        string.IsNullOrWhiteSpace(ResourceKey);

    /// <summary>
    /// Converts literal text to <see cref="HelpText"/>.
    /// </summary>
    /// <param name="text">The literal help text.</param>
    public static implicit operator HelpText(string text) => new(Text: text);

    /// <inheritdoc/>
    public override string ToString() => Text ?? ResourceKey ?? string.Empty;
}
