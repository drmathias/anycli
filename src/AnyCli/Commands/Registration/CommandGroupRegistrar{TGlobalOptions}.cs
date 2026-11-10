// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Registers commands inside a command group whose parameters derive from the configured global
/// options type.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
public sealed class CommandGroupRegistrar<TGlobalOptions>
    where TGlobalOptions : class
{
    private readonly CommandRegistrar<TGlobalOptions> _commands;

    internal CommandGroupRegistrar(CommandRegistrar<TGlobalOptions> commands)
    {
        _commands = commands;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed parameters.
    /// </summary>
    public CommandGroupRegistrar<TGlobalOptions> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _commands.AddCommand<TCommand, TParameters>(name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed parameters.
    /// </summary>
    public CommandGroupRegistrar<TGlobalOptions> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _commands.AddAsyncCommand<TCommand, TParameters>(name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command with typed parameters that also has subcommands.
    /// </summary>
    public CommandGroupRegistrar<TGlobalOptions> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions>> configureChildren)
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _commands.AddCommand<TCommand, TParameters>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command with typed parameters that also has subcommands.
    /// </summary>
    public CommandGroupRegistrar<TGlobalOptions> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions>> configureChildren)
        where TCommand : class, IAsyncCommand<TParameters>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _commands.AddAsyncCommand<TCommand, TParameters>(name, description, configureChildren);
        return this;
    }

    /// <summary>
    /// Registers a nested command group.
    /// </summary>
    public CommandGroupRegistrar<TGlobalOptions> AddGroup(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions>> configure)
    {
        _commands.AddGroup(name, description, configure);
        return this;
    }
}
