// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli;

/// <summary>
/// Registers commands that return a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public sealed class CommandConfigurator<TExitCode>
    where TExitCode : struct, Enum
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
    public CommandConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyCommand<TExitCode>
    {
        _builder.AddEmptyExitCodeCommand<TCommand, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, ICommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _builder.AddExitCodeCommand<TCommand, TSettings, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyAsyncCommand<TExitCode>
    {
        _builder.AddEmptyExitCodeAsyncCommand<TCommand, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _builder.AddExitCodeAsyncCommand<TCommand, TSettings, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : EmptyCommand<TExitCode>
    {
        var command = _builder.AddEmptyExitCodeCommand<TCommand, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : class, ICommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        var command = _builder.AddExitCodeCommand<TCommand, TSettings, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : EmptyAsyncCommand<TExitCode>
    {
        var command = _builder.AddEmptyExitCodeAsyncCommand<TCommand, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : class, IAsyncCommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        var command = _builder.AddExitCodeAsyncCommand<TCommand, TSettings, TExitCode>(_target, name, description);
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
    public CommandConfigurator<TExitCode> AddGroup(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var group = _builder.AddGroup(_target, name, description);
        configure(new CommandGroupConfigurator<TExitCode>(
            new CommandConfigurator<TExitCode>(_builder, group)));
        return this;
    }

    private CommandConfigurator<TExitCode> ConfigureChildren(
        CommandNode command,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
    {
        ArgumentNullException.ThrowIfNull(configureChildren);
        ConsoleAppBuilder.EnsureCanHaveSubcommands(command);
        configureChildren(new CommandGroupConfigurator<TExitCode>(
            new CommandConfigurator<TExitCode>(_builder, command)));
        return this;
    }
}
