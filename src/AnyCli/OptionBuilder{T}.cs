// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Configures a single named option.
/// </summary>
/// <typeparam name="T">The option value or collection type.</typeparam>
public sealed class OptionBuilder<T>
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
    public OptionBuilder<T> SetAliases(params IReadOnlyCollection<string> aliases)
    {
        SetAliasesCore(_option, aliases);
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
    public OptionBuilder<T> SetDescription(string description)
    {
        SetDescriptionCore(_option, description);
        return this;
    }

    /// <summary>
    /// Marks the option as required.
    /// </summary>
    /// <returns>A builder that does not allow a default value.</returns>
    public RequiredOptionBuilder<T> SetRequired()
    {
        _option.Required = true;
        return new RequiredOptionBuilder<T>(_option);
    }

    /// <summary>
    /// Sets the default value used when the option is not supplied.
    /// </summary>
    /// <param name="value">The default value.</param>
    /// <returns>A builder that does not allow the option to be required.</returns>
    public DefaultValueOptionBuilder<T> SetDefaultValue(T value)
    {
        _option.SetDefaultValue(value);
        return new DefaultValueOptionBuilder<T>(_option);
    }

    internal static void SetAliasesCore(
        OptionDefinition option,
        IReadOnlyCollection<string> aliases)
    {
        ArgumentNullException.ThrowIfNull(aliases);
        option.SetAliases([.. aliases.Select(CommandOptionBuilder.NormalizeName)]);
    }

    internal static void SetDescriptionCore(
        OptionDefinition option,
        string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        option.Description = description;
    }
}

/// <summary>
/// Configures a required named option.
/// </summary>
/// <typeparam name="T">The option value or collection type.</typeparam>
public sealed class RequiredOptionBuilder<T>
{
    private readonly OptionDefinition _option;

    internal RequiredOptionBuilder(OptionDefinition option)
    {
        _option = option;
    }

    /// <summary>
    /// Sets the additional names for the option.
    /// </summary>
    /// <param name="aliases">The additional option names.</param>
    /// <returns>The current option builder.</returns>
    public RequiredOptionBuilder<T> SetAliases(params IReadOnlyCollection<string> aliases)
    {
        OptionBuilder<T>.SetAliasesCore(_option, aliases);
        return this;
    }

    /// <summary>
    /// Sets the description shown in help output.
    /// </summary>
    /// <param name="description">The option description.</param>
    /// <returns>The current option builder.</returns>
    public RequiredOptionBuilder<T> SetDescription(string description)
    {
        OptionBuilder<T>.SetDescriptionCore(_option, description);
        return this;
    }
}

/// <summary>
/// Configures a named option that has a default value.
/// </summary>
/// <typeparam name="T">The option value or collection type.</typeparam>
public sealed class DefaultValueOptionBuilder<T>
{
    private readonly OptionDefinition _option;

    internal DefaultValueOptionBuilder(OptionDefinition option)
    {
        _option = option;
    }

    /// <summary>
    /// Sets the additional names for the option.
    /// </summary>
    /// <param name="aliases">The additional option names.</param>
    /// <returns>The current option builder.</returns>
    public DefaultValueOptionBuilder<T> SetAliases(params IReadOnlyCollection<string> aliases)
    {
        OptionBuilder<T>.SetAliasesCore(_option, aliases);
        return this;
    }

    /// <summary>
    /// Sets the description shown in help output.
    /// </summary>
    /// <param name="description">The option description.</param>
    /// <returns>The current option builder.</returns>
    public DefaultValueOptionBuilder<T> SetDescription(string description)
    {
        OptionBuilder<T>.SetDescriptionCore(_option, description);
        return this;
    }
}
