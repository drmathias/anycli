// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

/// <summary>
/// Provides the parsed positional argument values for a command.
/// </summary>
public sealed class CommandArgumentValues
{
    private readonly IReadOnlyDictionary<string, ArgumentArity> _arities;
    private readonly IReadOnlySet<string>? _supplied;
    private readonly IReadOnlyDictionary<string, object?> _values;

    internal CommandArgumentValues(
        IReadOnlyDictionary<string, ArgumentArity> arities,
        IReadOnlyDictionary<string, object?> values,
        IReadOnlySet<string>? supplied)
    {
        _arities = arities;
        _values = values;
        _supplied = supplied;
    }

    /// <summary>
    /// Gets the value of a required argument.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument value.</returns>
    /// <exception cref="InvalidOperationException">
    /// No argument named <paramref name="name"/> is defined, or it is not a required argument.
    /// </exception>
    public string GetRequired(string name)
    {
        return GetArity(name) != ArgumentArity.ExactlyOne
            ? throw new InvalidOperationException(
                $"Argument '{name}' does not accept exactly one required value.")
            : (string)_values[name]!;
    }

    /// <summary>
    /// Gets the value of an optional argument.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument value, or <see langword="null"/> when no default was supplied.</returns>
    /// <exception cref="InvalidOperationException">
    /// No argument named <paramref name="name"/> is defined, or it is not an optional argument.
    /// </exception>
    public string? GetOptional(string name)
    {
        return GetArity(name) != ArgumentArity.ZeroOrOne
            ? throw new InvalidOperationException(
                $"Argument '{name}' does not accept zero or one value.")
            : (string?)_values[name];
    }

    /// <summary>
    /// Attempts to get an optional argument that was explicitly supplied.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The supplied value, or the parser's default when not supplied.</param>
    /// <returns><see langword="true"/> when the argument was supplied; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// No argument named <paramref name="name"/> is defined, or it is not an optional argument.
    /// </exception>
    public bool TryGetOptional(string name, out string? value)
    {
        if (GetArity(name) != ArgumentArity.ZeroOrOne)
        {
            throw new InvalidOperationException(
                $"Argument '{name}' does not accept zero or one value.");
        }

        value = (string?)_values[name];
        return _supplied?.Contains(name) == true;
    }

    /// <summary>
    /// Gets the values of a variadic argument.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument values, or an empty list when none were supplied.</returns>
    /// <exception cref="InvalidOperationException">
    /// No argument named <paramref name="name"/> is defined, or it is not a variadic argument.
    /// </exception>
    public IReadOnlyList<string> GetVariadic(string name)
    {
        return !GetArity(name).IsVariadic
            ? throw new InvalidOperationException(
                $"Argument '{name}' does not accept multiple values.")
            : (IReadOnlyList<string>)_values[name]!;
    }

    private ArgumentArity GetArity(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return !_arities.TryGetValue(name, out var arity)
            ? throw new InvalidOperationException($"No argument named '{name}' is defined for this command.")
            : arity;
    }
}
