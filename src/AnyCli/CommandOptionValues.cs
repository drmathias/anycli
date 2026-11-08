// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the parsed named option values for a command.
/// </summary>
public sealed class CommandOptionValues
{
    private readonly IReadOnlyDictionary<string, OptionValue> _defaults;
    private readonly IReadOnlyDictionary<string, OptionValue> _values;

    internal CommandOptionValues(
        IReadOnlyDictionary<string, OptionValue> values,
        IReadOnlyDictionary<string, OptionValue>? defaults = null)
    {
        _values = values;
        _defaults = defaults ?? new Dictionary<string, OptionValue>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets a value indicating whether the option was supplied.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <returns><see langword="true"/> when the option was supplied; otherwise, <see langword="false"/>.</returns>
    public bool Has(string name)
    {
        return _values.ContainsKey(NormalizeName(name));
    }

    /// <summary>
    /// Attempts to get the effective value of an option.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <param name="value">
    /// The last supplied value or configured default, or <see langword="null"/> when the option
    /// was supplied without a value or has a null default.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the option was supplied or has a default; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool TryGetValue(string name, out string? value)
    {
        var normalizedName = NormalizeName(name);
        if (!_values.TryGetValue(normalizedName, out var option) &&
            !_defaults.TryGetValue(normalizedName, out option))
        {
            value = null;
            return false;
        }

        value = option.Values.Count == 0 ? null : option.Values[^1];
        return true;
    }

    /// <summary>
    /// Gets every effective value for a repeated option.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <returns>
    /// The supplied or default values, or an empty list when the option was not supplied and has
    /// no default.
    /// </returns>
    public IReadOnlyList<string> GetValues(string name)
    {
        var normalizedName = NormalizeName(name);
        if (_values.TryGetValue(normalizedName, out var option))
        {
            return option.Values;
        }

        return _defaults.TryGetValue(normalizedName, out option) ? option.Values : [];
    }

    private static string NormalizeName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return name.Length == 0 || name[0] == '-'
            ? name
            : name.Length == 1
                ? "-" + name
                : "--" + name;
    }
}
