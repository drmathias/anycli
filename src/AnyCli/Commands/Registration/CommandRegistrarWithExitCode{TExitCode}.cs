// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Registers commands that return a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public sealed class CommandRegistrarWithExitCode<TExitCode>
    where TExitCode : struct, Enum
{
    private readonly ConsoleAppBuilder _builder;
    private readonly CommandNode _target;

    internal CommandRegistrarWithExitCode(ConsoleAppBuilder builder, CommandNode target)
    {
        _builder = builder;
        _target = target;
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
    public CommandRegistrarWithExitCode<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : NoParametersCommand<TExitCode>
    {
        _builder.AddNoParametersExitCodeCommand<TCommand, TExitCode>(_target, name, description);
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
    public CommandRegistrarWithExitCode<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        _builder.AddExitCodeCommand<TCommand, TParameters, TExitCode>(_target, name, description);
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
    public CommandRegistrarWithExitCode<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : NoParametersAsyncCommand<TExitCode>
    {
        _builder.AddNoParametersExitCodeAsyncCommand<TCommand, TExitCode>(_target, name, description);
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
    public CommandRegistrarWithExitCode<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        _builder.AddExitCodeAsyncCommand<TCommand, TParameters, TExitCode>(_target, name, description);
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
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configureChildren"/> is <see langword="null"/>.
    /// </exception>
    public CommandRegistrarWithExitCode<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupRegistrarWithExitCode<TExitCode>> configureChildren)
        where TCommand : NoParametersCommand<TExitCode>
    {
        var command = _builder.AddNoParametersExitCodeCommand<TCommand, TExitCode>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
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
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configureChildren"/> is <see langword="null"/>.
    /// </exception>
    public CommandRegistrarWithExitCode<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrarWithExitCode<TExitCode>> configureChildren)
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        var command = _builder.AddExitCodeCommand<TCommand, TParameters, TExitCode>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers an asynchronous command without parameters that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureChildren">The subcommand configuration callback.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configureChildren"/> is <see langword="null"/>.
    /// </exception>
    public CommandRegistrarWithExitCode<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupRegistrarWithExitCode<TExitCode>> configureChildren)
        where TCommand : NoParametersAsyncCommand<TExitCode>
    {
        var command = _builder.AddNoParametersExitCodeAsyncCommand<TCommand, TExitCode>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
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
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configureChildren"/> is <see langword="null"/>.
    /// </exception>
    public CommandRegistrarWithExitCode<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrarWithExitCode<TExitCode>> configureChildren)
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
    {
        var command = _builder.AddExitCodeAsyncCommand<TCommand, TParameters, TExitCode>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers a command group.
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
    public CommandRegistrarWithExitCode<TExitCode> AddGroup(
        string name,
        string description,
        Action<CommandGroupRegistrarWithExitCode<TExitCode>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var group = _builder.AddGroup(_target, name, description);
        configure(new CommandGroupRegistrarWithExitCode<TExitCode>(
            new CommandRegistrarWithExitCode<TExitCode>(_builder, group)));
        return this;
    }

    private CommandRegistrarWithExitCode<TExitCode> ConfigureChildren(
        CommandNode command,
        Action<CommandGroupRegistrarWithExitCode<TExitCode>> configureChildren)
    {
        ArgumentNullException.ThrowIfNull(configureChildren);
        ConsoleAppBuilder.EnsureCanHaveSubcommands(command);
        configureChildren(new CommandGroupRegistrarWithExitCode<TExitCode>(
            new CommandRegistrarWithExitCode<TExitCode>(_builder, command)));
        return this;
    }
}
