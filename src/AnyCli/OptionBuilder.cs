// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Configures a single named option.
/// </summary>
public sealed class OptionBuilder
{
    private readonly OptionDefinition _option;

    internal OptionBuilder(OptionDefinition option)
    {
        _option = option;
    }

    /// <summary>
    /// Sets the additional names for the option.
    /// </summary>
    /// <param name="aliases">The additional option names.</param>
    /// <returns>The current option builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="aliases"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An alias is <see langword="null"/>, empty, whitespace, or not a valid option name.
    /// </exception>
    public OptionBuilder SetAliases(params IReadOnlyCollection<string> aliases)
    {
        ArgumentNullException.ThrowIfNull(aliases);
        _option.SetAliases([.. aliases.Select(CommandOptionBuilder.NormalizeName)]);
        return this;
    }

    /// <summary>
    /// Sets the description shown in help output.
    /// </summary>
    /// <param name="description">The option description.</param>
    /// <returns>The current option builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="description"/> is <see langword="null"/>.
    /// </exception>
    public OptionBuilder SetDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        _option.Description = description;
        return this;
    }

    /// <summary>
    /// Sets whether the option must be supplied.
    /// </summary>
    /// <param name="required">Whether the option must be supplied.</param>
    /// <returns>The current option builder.</returns>
    public OptionBuilder SetRequired(bool required = true)
    {
        _option.Required = required;
        return this;
    }
}
