// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Registers commands whose parameters derive from the configured global options type.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
public sealed class CommandRegistrar<TGlobalOptions>
    where TGlobalOptions : class
{
    private readonly ConsoleAppBuilder _builder;
    private readonly CommandNode _target;

    internal CommandRegistrar(ConsoleAppBuilder builder, CommandNode target)
    {
        _builder = builder;
        _target = target;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current configurator.</returns>
    public CommandRegistrar<TGlobalOptions> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _builder.AddCommand<TCommand, TParameters>(_target, name, description);
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
    public CommandRegistrar<TGlobalOptions> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _builder.AddAsyncCommand<TCommand, TParameters>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command with typed parameters that also has subcommands.
    /// </summary>
    public CommandRegistrar<TGlobalOptions> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions>> configureChildren)
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        var command = _builder.AddCommand<TCommand, TParameters>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers an asynchronous command with typed parameters that also has subcommands.
    /// </summary>
    public CommandRegistrar<TGlobalOptions> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions>> configureChildren)
        where TCommand : class, IAsyncCommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        var command = _builder.AddAsyncCommand<TCommand, TParameters>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers a command group.
    /// </summary>
    public CommandRegistrar<TGlobalOptions> AddGroup(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var group = _builder.AddGroup(_target, name, description);
        configure(new CommandGroupRegistrar<TGlobalOptions>(
            new CommandRegistrar<TGlobalOptions>(_builder, group)));
        return this;
    }

    private CommandRegistrar<TGlobalOptions> ConfigureChildren(
        CommandNode command,
        Action<CommandGroupRegistrar<TGlobalOptions>> configureChildren)
    {
        ArgumentNullException.ThrowIfNull(configureChildren);
        ConsoleAppBuilder.EnsureCanHaveSubcommands(command);
        configureChildren(new CommandGroupRegistrar<TGlobalOptions>(
            new CommandRegistrar<TGlobalOptions>(_builder, command)));
        return this;
    }
}
