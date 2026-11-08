// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

/// <summary>
/// Defines the named options accepted by a command.
/// </summary>
public sealed class CommandOptionBuilder
{
    private readonly List<OptionDefinition> _options = [];

    /// <summary>
    /// Adds an option that consumes a value.
    /// </summary>
    /// <typeparam name="T">The option value type.</typeparam>
    /// <param name="name">The primary option name.</param>
    /// <returns>The option builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public OptionBuilder<T> AddValue<T>(string name) =>
        Add<T>(name, typeof(T), typeof(T), takesValue: true, isCollection: false);

    /// <summary>
    /// Adds an option that does not consume a value.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <returns>The option builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public OptionBuilder<bool> AddFlag(string name) =>
        Add<bool>(name, typeof(bool), typeof(bool), takesValue: false, isCollection: false);

    /// <summary>
    /// Adds an option that can be repeated into a collection.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    /// <param name="name">The primary option name.</param>
    /// <returns>The option builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public OptionBuilder<T[]> AddCollection<T>(string name) =>
        Add<T[]>(name, typeof(T), typeof(T[]), takesValue: true, isCollection: true);

    internal IReadOnlyList<OptionDefinition> Build()
    {
        Validate();
        return [.. _options];
    }

    internal static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return name[0] == '-' ? name : name.Length == 1 ? "-" + name : "--" + name;
    }

    private OptionBuilder<TValue> Add<TValue>(
        string name,
        Type valueType,
        Type declaredType,
        bool takesValue,
        bool isCollection)
    {
        var option = new OptionDefinition(
            NormalizeName(name),
            [],
            string.Empty,
            valueType,
            declaredType,
            required: false,
            takesValue,
            isCollection);
        _options.Add(option);
        return new OptionBuilder<TValue>(option);
    }

    private void Validate()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var option in _options)
        {
            foreach (var name in option.Names)
            {
                if (!names.Add(name))
                {
                    throw new InvalidOperationException($"Duplicate option name '{name}'.");
                }
            }
        }
    }
}
