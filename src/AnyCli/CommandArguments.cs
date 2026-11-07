// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the parsed positional arguments for a command.
/// </summary>
public sealed class CommandArguments
{
    private readonly IReadOnlyDictionary<string, ArgumentKind> _kinds;
    private readonly IReadOnlySet<string> _supplied;
    private readonly IReadOnlyDictionary<string, object?> _values;

    internal CommandArguments(
        IReadOnlyDictionary<string, ArgumentKind> kinds,
        IReadOnlyDictionary<string, object?> values,
        IReadOnlySet<string> supplied)
    {
        _kinds = kinds;
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
        EnsureKind(name, ArgumentKind.Required);
        return (string)_values[name]!;
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
        EnsureKind(name, ArgumentKind.Optional);
        return (string?)_values[name];
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
        EnsureKind(name, ArgumentKind.Optional);
        value = (string?)_values[name];
        return _supplied.Contains(name);
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
        EnsureKind(name, ArgumentKind.Variadic);
        return (IReadOnlyList<string>)_values[name]!;
    }

    private void EnsureKind(string name, ArgumentKind expected)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!_kinds.TryGetValue(name, out var actual))
        {
            throw new InvalidOperationException($"No argument named '{name}' is defined for this command.");
        }

        if (actual != expected)
        {
            throw new InvalidOperationException(
                $"Argument '{name}' is {actual.ToString().ToLowerInvariant()}, not {expected.ToString().ToLowerInvariant()}.");
        }
    }
}
