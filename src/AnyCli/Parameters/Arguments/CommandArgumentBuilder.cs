// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

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
        Add(name, ArgumentArity.ExactlyOne, typeof(string), static () => null);

    /// <summary>
    /// Adds a required argument with the specified value type.
    /// </summary>
    /// <typeparam name="T">The argument value type.</typeparam>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder Add<T>(string name) =>
        Add(name, ArgumentArity.ExactlyOne, typeof(T), static () => null);

    /// <summary>
    /// Adds an optional argument that defaults to <see langword="null"/>.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder AddOptional(string name) =>
        Add(name, ArgumentArity.ZeroOrOne, typeof(string), static () => null);

    /// <summary>
    /// Adds an optional argument with the specified value type that defaults to
    /// <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The argument value type.</typeparam>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder AddOptional<T>(string name) =>
        Add(name, ArgumentArity.ZeroOrOne, typeof(T), static () => null);

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
        return Add(name, ArgumentArity.ZeroOrOne, typeof(string), () => defaultValue);
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
        return Add(name, ArgumentArity.ZeroOrOne, typeof(string), defaultValueFactory);
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
        AddVariadic(name, ArgumentArity.ZeroOrMore);

    /// <summary>
    /// Adds a variadic argument with the specified arity.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="arity">The number of values the argument accepts.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// A variadic argument must be the last argument.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace, or
    /// <paramref name="arity"/> does not accept multiple values.
    /// </exception>
    public CommandArgumentBuilder AddVariadic(string name, ArgumentArity arity) =>
        AddVariadicCore(name, arity, typeof(string[]), static () => null);

    /// <summary>
    /// Adds a variadic argument with the specified element type that consumes all
    /// remaining values.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="name">The argument name.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// A variadic argument must be the last argument.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandArgumentBuilder AddVariadic<T>(string name) =>
        AddVariadic<T>(name, ArgumentArity.ZeroOrMore);

    /// <summary>
    /// Adds a variadic argument with the specified element type and arity.
    /// </summary>
    /// <typeparam name="T">The array element type.</typeparam>
    /// <param name="name">The argument name.</param>
    /// <param name="arity">The number of values the argument accepts.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// A variadic argument must be the last argument.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace, or
    /// <paramref name="arity"/> does not accept multiple values.
    /// </exception>
    public CommandArgumentBuilder AddVariadic<T>(string name, ArgumentArity arity) =>
        AddVariadicCore(name, arity, typeof(T[]), static () => null);

    /// <summary>
    /// Adds a variadic argument with the specified element type, collection type, and arity.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    /// <param name="name">The argument name.</param>
    /// <param name="collectionType">The declared collection type.</param>
    /// <param name="arity">The number of values the argument accepts.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// A variadic argument must be the last argument.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace, or
    /// <paramref name="arity"/> does not accept multiple values.
    /// </exception>
    public CommandArgumentBuilder AddVariadic<T>(
        string name,
        Type collectionType,
        ArgumentArity arity)
    {
        ArgumentNullException.ThrowIfNull(collectionType);
        return AddVariadicCore(name, arity, collectionType, static () => null);
    }

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

    /// <summary>
    /// Sets the default value of the most recently added optional argument.
    /// </summary>
    /// <typeparam name="T">The argument value type.</typeparam>
    /// <param name="value">The default value.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="InvalidOperationException">
    /// No argument has been added, the argument is required, or the argument is variadic.
    /// </exception>
    public CommandArgumentBuilder SetDefaultValue<T>(T value)
    {
        if (_arguments.Count == 0)
        {
            throw new InvalidOperationException("No argument has been added.");
        }

        var argument = _arguments[^1];
        if (argument.Arity.IsRequired || argument.Arity.IsVariadic)
        {
            throw new InvalidOperationException(
                "Only non-variadic optional arguments can have a default value.");
        }

        argument.SetDefaultValue(value);
        return this;
    }

    /// <summary>
    /// Sets the default values of the most recently added zero-or-more variadic argument.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="values">The default values.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// No argument has been added, the argument is not variadic, or the argument requires values.
    /// </exception>
    public CommandArgumentBuilder SetDefaultValues<T>(IReadOnlyCollection<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (_arguments.Count == 0)
        {
            throw new InvalidOperationException("No argument has been added.");
        }

        var argument = _arguments[^1];
        if (!argument.Arity.IsVariadic || argument.Arity.IsRequired)
        {
            throw new InvalidOperationException(
                "Only zero-or-more variadic arguments can have default values.");
        }

        argument.SetDefaultValue(values.ToArray());
        return this;
    }

    internal IReadOnlyList<ArgumentDefinition> Build()
    {
        Validate();
        return _arguments.ToArray();
    }

    private CommandArgumentBuilder Add(
        string name,
        ArgumentArity arity,
        Type argumentType,
        Func<string?> defaultFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _arguments.Add(new ArgumentDefinition(name, arity, argumentType, defaultFactory));
        return this;
    }

    private CommandArgumentBuilder AddVariadicCore(
        string name,
        ArgumentArity arity,
        Type argumentType,
        Func<string?> defaultFactory)
    {
        if (!arity.IsVariadic)
        {
            throw new ArgumentException(
                "A variadic argument must accept more than one value.",
                nameof(arity));
        }

        return Add(name, arity, argumentType, defaultFactory);
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

            if (argument.Arity.IsRequired && !argument.Arity.IsVariadic && optionalSeen)
            {
                throw new InvalidOperationException(
                    "Required arguments must appear before optional arguments.");
            }

            if (!argument.Arity.IsVariadic && !argument.Arity.IsRequired)
            {
                optionalSeen = true;
            }

            if (argument.Arity.IsVariadic)
            {
                variadicSeen = true;
            }
        }
    }
}
