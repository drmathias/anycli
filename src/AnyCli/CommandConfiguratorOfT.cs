// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
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

    internal CommandConfigurator(ConsoleAppBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Registers a synchronous command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureArguments">
    /// An optional callback that defines the command's positional arguments.
    /// </param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandConfigurator<TExitCode> AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "",
        Action<CommandArgumentBuilder>? configureArguments = null)
        where TCommand : class, IExitCodeCommand<TExitCode>
    {
        _builder.AddCommand<TCommand>(name, description, configureArguments);
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
        _builder.AddExitCodeCommand<TCommand, TSettings, TExitCode>(name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <param name="configureArguments">
    /// An optional callback that defines the command's positional arguments.
    /// </param>
    /// <returns>The current configurator.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public CommandConfigurator<TExitCode> AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "",
        Action<CommandArgumentBuilder>? configureArguments = null)
        where TCommand : class, IAsyncExitCodeCommand<TExitCode>
    {
        _builder.AddAsyncCommand<TCommand>(name, description, configureArguments);
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
        _builder.AddExitCodeAsyncCommand<TCommand, TSettings, TExitCode>(name, description);
        return this;
    }
}
