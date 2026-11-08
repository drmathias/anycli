// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli;

/// <summary>
/// Registers commands inside a command group that returns a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public sealed class CommandGroupConfigurator<TExitCode>
    where TExitCode : struct, Enum
{
    private readonly CommandConfigurator<TExitCode> _commands;

    internal CommandGroupConfigurator(CommandConfigurator<TExitCode> commands)
    {
        _commands = commands;
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
    public CommandGroupConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyCommand<TExitCode>
    {
        _commands.AddCommand<TCommand>(name, description);
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
    public CommandGroupConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, ICommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _commands.AddCommand<TCommand, TSettings>(name, description);
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
    public CommandGroupConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyAsyncCommand<TExitCode>
    {
        _commands.AddAsyncCommand<TCommand>(name, description);
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
    public CommandGroupConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _commands.AddAsyncCommand<TCommand, TSettings>(name, description);
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
    public CommandGroupConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : EmptyCommand<TExitCode>
    {
        _commands.AddCommand<TCommand>(name, description, configureChildren);
        return this;
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
    public CommandGroupConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : class, ICommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _commands.AddCommand<TCommand, TSettings>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command without settings that also has subcommands.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureChildren">The subcommand configuration callback.</param>
    /// <returns>The current configurator.</returns>
    public CommandGroupConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : EmptyAsyncCommand<TExitCode>
    {
        _commands.AddAsyncCommand<TCommand>(name, description, configureChildren);
        return this;
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
    public CommandGroupConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configureChildren)
        where TCommand : class, IAsyncCommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
    {
        _commands.AddAsyncCommand<TCommand, TSettings>(name, description, configureChildren);
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
    public CommandGroupConfigurator<TExitCode> AddGroup(
        string name,
        string description,
        Action<CommandGroupConfigurator<TExitCode>> configure)
    {
        _commands.AddGroup(name, description, configure);
        return this;
    }
}
