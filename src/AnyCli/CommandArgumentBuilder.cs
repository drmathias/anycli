// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Defines the positional arguments accepted by a command.
/// </summary>
public sealed class CommandArgumentBuilder
{
    private readonly List<ArgumentDefinition> _arguments = [];

    /// <summary>
    /// Adds a required argument.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder Add(string name) =>
        Add(name, ArgumentKind.Required, static () => null);

    /// <summary>
    /// Adds an optional argument that defaults to <see langword="null"/>.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder AddOptional(string name) =>
        Add(name, ArgumentKind.Optional, static () => null);

    /// <summary>
    /// Adds an optional argument with a constant default value.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="defaultValue">The value used when the argument is not supplied.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="defaultValue"/> is <see langword="null"/>.
    /// </exception>
    public CommandArgumentBuilder AddOptional(string name, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(defaultValue);
        return Add(name, ArgumentKind.Optional, () => defaultValue);
    }

    /// <summary>
    /// Adds an optional argument with a computed default value.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="defaultValueFactory">
    /// A factory invoked when the argument is not supplied.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="defaultValueFactory"/> is <see langword="null"/>.
    /// </exception>
    public CommandArgumentBuilder AddOptional(string name, Func<string?> defaultValueFactory)
    {
        ArgumentNullException.ThrowIfNull(defaultValueFactory);
        return Add(name, ArgumentKind.Optional, defaultValueFactory);
    }

    /// <summary>
    /// Adds a variadic argument that consumes all remaining values.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// A variadic argument must be the last argument.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder AddVariadic(string name) =>
        Add(name, ArgumentKind.Variadic, static () => null);

    /// <summary>
    /// Sets the description of the most recently added argument.
    /// </summary>
    /// <param name="description">The argument description.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="description"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// No argument has been added.
    /// </exception>
    public CommandArgumentBuilder Describe(string description)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (_arguments.Count == 0)
        {
            throw new InvalidOperationException("No argument has been added.");
        }

        _arguments[^1].Description = description;
        return this;
    }

    internal IReadOnlyList<ArgumentDefinition> Build()
    {
        Validate();
        return _arguments.ToArray();
    }

    private CommandArgumentBuilder Add(string name, ArgumentKind kind, Func<string?> defaultFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _arguments.Add(new ArgumentDefinition(name, kind, defaultFactory));
        return this;
    }

    private void Validate()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var optionalSeen = false;
        var variadicSeen = false;

        foreach (var argument in _arguments)
        {
            if (!names.Add(argument.Name))
            {
                throw new InvalidOperationException($"Duplicate argument '{argument.Name}'.");
            }

            if (variadicSeen)
            {
                throw new InvalidOperationException("A variadic argument must be the last argument.");
            }

            if (argument.Kind == ArgumentKind.Required && optionalSeen)
            {
                throw new InvalidOperationException(
                    "Required arguments must appear before optional arguments.");
            }

            if (argument.Kind == ArgumentKind.Optional)
            {
                optionalSeen = true;
            }

            if (argument.Kind == ArgumentKind.Variadic)
            {
                variadicSeen = true;
            }
        }
    }
}
