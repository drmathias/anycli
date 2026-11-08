// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Defines the named options accepted by a command.
/// </summary>
public sealed class CommandOptionBuilder
{
    private readonly List<OptionDefinition> _options = [];

    /// <summary>
    /// Adds an option.
    /// </summary>
    /// <typeparam name="T">The option value or collection element type.</typeparam>
    /// <param name="name">The primary option name.</param>
    /// <param name="aliases">Additional option names.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="required">Whether the option must be supplied.</param>
    /// <param name="takesValue">Whether the option consumes a value.</param>
    /// <param name="isCollection">Whether the option can be repeated into a collection.</param>
    /// <param name="collectionType">The declared collection type.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, whitespace, or not a valid
    /// option name.
    /// </exception>
    public CommandOptionBuilder Add<T>(
        string name,
        string[]? aliases = null,
        string description = "",
        bool required = false,
        bool takesValue = true,
        bool isCollection = false,
        Type? collectionType = null)
    {
        var normalizedName = NormalizeName(name);
        var normalizedAliases = (aliases ?? [])
            .Select(NormalizeName)
            .ToArray();
        var declaredType = isCollection
            ? collectionType ?? typeof(T[])
            : typeof(T);

        _options.Add(new OptionDefinition(
            normalizedName,
            normalizedAliases,
            description,
            typeof(T),
            declaredType,
            required,
            takesValue,
            isCollection));
        return this;
    }

    /// <summary>
    /// Sets the description of the most recently added option.
    /// </summary>
    /// <param name="description">The option description.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="description"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// No option has been added.
    /// </exception>
    public CommandOptionBuilder Describe(string description)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (_options.Count == 0)
        {
            throw new InvalidOperationException("No option has been added.");
        }

        _options[^1].Description = description;
        return this;
    }

    internal IReadOnlyList<OptionDefinition> Build()
    {
        Validate();
        return _options.ToArray();
    }

    private static string NormalizeName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name[0] == '-')
        {
            return name;
        }

        return name.Length == 1 ? "-" + name : "--" + name;
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
