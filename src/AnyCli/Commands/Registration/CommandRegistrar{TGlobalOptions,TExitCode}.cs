// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Registers commands whose parameters derive from the configured global options type and return a
/// custom exit code enum.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public sealed class CommandRegistrar<TGlobalOptions, TExitCode>
    where TGlobalOptions : class
    where TExitCode : struct, Enum
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
    public CommandRegistrar<TGlobalOptions, TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _builder.AddExitCodeCommand<TCommand, TParameters, TExitCode>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed parameters.
    /// </summary>
    public CommandRegistrar<TGlobalOptions, TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        _builder.AddExitCodeAsyncCommand<TCommand, TParameters, TExitCode>(_target, name, description);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command with typed parameters that also has subcommands.
    /// </summary>
    public CommandRegistrar<TGlobalOptions, TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions, TExitCode>> configureChildren)
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        var command = _builder.AddExitCodeCommand<TCommand, TParameters, TExitCode>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers an asynchronous command with typed parameters that also has subcommands.
    /// </summary>
    public CommandRegistrar<TGlobalOptions, TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions, TExitCode>> configureChildren)
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, TGlobalOptions, ICommandParameters<TParameters>
    {
        var command = _builder.AddExitCodeAsyncCommand<TCommand, TParameters, TExitCode>(_target, name, description);
        return ConfigureChildren(command, configureChildren);
    }

    /// <summary>
    /// Registers a command group.
    /// </summary>
    public CommandRegistrar<TGlobalOptions, TExitCode> AddGroup(
        string name,
        string description,
        Action<CommandGroupRegistrar<TGlobalOptions, TExitCode>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var group = _builder.AddGroup(_target, name, description);
        configure(new CommandGroupRegistrar<TGlobalOptions, TExitCode>(
            new CommandRegistrar<TGlobalOptions, TExitCode>(_builder, group)));
        return this;
    }

    private CommandRegistrar<TGlobalOptions, TExitCode> ConfigureChildren(
        CommandNode command,
        Action<CommandGroupRegistrar<TGlobalOptions, TExitCode>> configureChildren)
    {
        ArgumentNullException.ThrowIfNull(configureChildren);
        ConsoleAppBuilder.EnsureCanHaveSubcommands(command);
        configureChildren(new CommandGroupRegistrar<TGlobalOptions, TExitCode>(
            new CommandRegistrar<TGlobalOptions, TExitCode>(_builder, command)));
        return this;
    }
}
