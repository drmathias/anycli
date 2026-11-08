// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Defines options that apply to every command in the application.
/// </summary>
public sealed class GlobalOptionBuilder
{
    private readonly CommandOptionBuilder _options = new();

    /// <summary>
    /// Adds a global option that consumes a value.
    /// </summary>
    /// <typeparam name="T">The option value type.</typeparam>
    /// <param name="name">The primary option name.</param>
    /// <returns>The option builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public OptionBuilder<T> AddValue<T>(string name) => _options.AddValue<T>(name);

    /// <summary>
    /// Adds a global option that does not consume a value.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <returns>The option builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public OptionBuilder<bool> AddFlag(string name) => _options.AddFlag(name);

    /// <summary>
    /// Adds a global option that can be repeated into a collection.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    /// <param name="name">The primary option name.</param>
    /// <returns>The option builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public OptionBuilder<T[]> AddCollection<T>(string name) => _options.AddCollection<T>(name);

    internal IReadOnlyList<OptionDefinition> Build() => _options.Build();
}
