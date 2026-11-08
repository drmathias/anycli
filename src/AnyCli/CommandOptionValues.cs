// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the parsed named option values for a command.
/// </summary>
public sealed class CommandOptionValues
{
    private readonly IReadOnlyDictionary<string, OptionValue> _values;

    internal CommandOptionValues(IReadOnlyDictionary<string, OptionValue> values)
    {
        _values = values;
    }

    /// <summary>
    /// Gets a value indicating whether the option was supplied.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <returns><see langword="true"/> when the option was supplied; otherwise, <see langword="false"/>.</returns>
    public bool Has(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _values.ContainsKey(name);
    }

    /// <summary>
    /// Attempts to get the last value supplied for an option.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <param name="value">
    /// The last supplied value, or <see langword="null"/> when the option was supplied without a
    /// value.
    /// </param>
    /// <returns><see langword="true"/> when the option was supplied; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(string name, out string? value)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!_values.TryGetValue(name, out var option))
        {
            value = null;
            return false;
        }

        value = option.Values.Count == 0 ? null : option.Values[^1];
        return true;
    }

    /// <summary>
    /// Gets every value supplied for a repeated option.
    /// </summary>
    /// <param name="name">The primary option name.</param>
    /// <returns>The supplied values, or an empty list when the option was not supplied.</returns>
    public IReadOnlyList<string> GetValues(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _values.TryGetValue(name, out var option) ? option.Values : [];
    }
}
