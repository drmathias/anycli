// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Registers commands inside a command group that returns a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public sealed class CommandGroupRegistrar<TExitCode>
    where TExitCode : struct, Enum
{
    private readonly CommandRegistrar<TExitCode> _commands;

    internal CommandGroupRegistrar(CommandRegistrar<TExitCode> commands)
    {
        _commands = commands;
    }

    /// <summary>
    /// Registers a synchronous command that does not use parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandGroupRegistrar<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : NoParametersCommand<TExitCode>
    {
        _commands.AddCommand<TCommand>(name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandGroupRegistrar<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        _commands.AddCommand<TCommand, TParameters>(name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that does not use parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandGroupRegistrar<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : NoParametersAsyncCommand<TExitCode>
    {
        _commands.AddAsyncCommand<TCommand>(name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandGroupRegistrar<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        _commands.AddAsyncCommand<TCommand, TParameters>(name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command without parameters that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureChildren">The subcommand configuration callback.</param>
    /// <returns>The current configurator.</returns>
    public CommandGroupRegistrar<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TExitCode>> configureChildren)
        where TCommand : NoParametersCommand<TExitCode>
    {
        _commands.AddCommand<TCommand>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command with typed parameters that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureChildren">The subcommand configuration callback.</param>
    /// <returns>The current configurator.</returns>
    public CommandGroupRegistrar<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TExitCode>> configureChildren)
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        _commands.AddCommand<TCommand, TParameters>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command without parameters that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureChildren">The subcommand configuration callback.</param>
    /// <returns>The current configurator.</returns>
    public CommandGroupRegistrar<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TExitCode>> configureChildren)
        where TCommand : NoParametersAsyncCommand<TExitCode>
    {
        _commands.AddAsyncCommand<TCommand>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command with typed parameters that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureChildren">The subcommand configuration callback.</param>
    /// <returns>The current configurator.</returns>
    public CommandGroupRegistrar<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TExitCode>> configureChildren)
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        _commands.AddAsyncCommand<TCommand, TParameters>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers a nested command group.
    /// </summary>
    /// <param name="name">The group name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configure">The group configuration callback.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public CommandGroupRegistrar<TExitCode> AddGroup(
        string name,
        string description,
        Action<CommandGroupRegistrar<TExitCode>> configure)
    {
        _commands.AddGroup(name, description, configure);
        return this;
    }
}
