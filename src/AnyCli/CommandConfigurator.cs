// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli;

/// <summary>
/// Registers commands that return standard integer exit codes.
/// </summary>
public sealed class CommandConfigurator
{
    private readonly ConsoleAppBuilder _builder;
    private readonly CommandNode _target;

    internal CommandConfigurator(ConsoleAppBuilder builder, CommandNode target)
    {
        _builder = builder;
        _target = target;
    }

    /// <summary>
    /// Registers a synchronous command that does not use settings.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandConfigurator AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyCommand
    {
        _builder.AddEmptyCommand<TCommand>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed settings.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandConfigurator AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _builder.AddCommand<TCommand, TSettings>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that does not use settings.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandConfigurator AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyAsyncCommand
    {
        _builder.AddEmptyAsyncCommand<TCommand>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed settings.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandConfigurator AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _builder.AddAsyncCommand<TCommand, TSettings>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command without settings that also has subcommands.
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
    public CommandConfigurator AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupConfigurator> configureChildren)
        where TCommand : EmptyCommand
    {
        var command = _builder.AddEmptyCommand<TCommand>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers a synchronous command with typed settings that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
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
    public CommandConfigurator AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(
        string name,
        string description,
        Action<CommandGroupConfigurator> configureChildren)
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        var command = _builder.AddCommand<TCommand, TSettings>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers an asynchronous command without settings that also has subcommands.
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
    public CommandConfigurator AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupConfigurator> configureChildren)
        where TCommand : EmptyAsyncCommand
    {
        var command = _builder.AddEmptyAsyncCommand<TCommand>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers an asynchronous command with typed settings that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
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
    public CommandConfigurator AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(
        string name,
        string description,
        Action<CommandGroupConfigurator> configureChildren)
        where TCommand : class, IAsyncCommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        var command = _builder.AddAsyncCommand<TCommand, TSettings>(_target, name, description);
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
    public CommandConfigurator AddGroup(
        string name,
        string description,
        Action<CommandGroupConfigurator> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var group = _builder.AddGroup(_target, name, description);
        configure(new CommandGroupConfigurator(new CommandConfigurator(_builder, group)));
        return this;
    }

    private CommandConfigurator ConfigureChildren(
        CommandNode command,
        Action<CommandGroupConfigurator> configureChildren)
    {
        ArgumentNullException.ThrowIfNull(configureChildren);
        ConsoleAppBuilder.EnsureCanHaveSubcommands(command);
        configureChildren(new CommandGroupConfigurator(new CommandConfigurator(_builder, command)));
        return this;
    }
}
