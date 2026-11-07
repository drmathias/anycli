// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli;

/// <summary>
/// Configures and creates a <see cref="ConsoleApp"/>.
/// </summary>
public sealed class ConsoleAppBuilder
{
    private readonly Dictionary<string, Func<IServiceProvider?, CommandContext, Task<int>>> _commands = new(StringComparer.Ordinal);

    private readonly Dictionary<string, CommandInfo> _commandInfos = new(StringComparer.Ordinal);
    private IHelpProvider? _helpProvider;
    private IServiceProvider? _serviceProvider;
    private string? _name;
    private string? _version;

    private ConsoleAppBuilder()
    {
    }

    /// <summary>
    /// Creates a new builder.
    /// </summary>
    /// <returns>A new <see cref="ConsoleAppBuilder"/> instance.</returns>
    public static ConsoleAppBuilder Create() => new();

    /// <summary>
    /// Sets the application name.
    /// </summary>
    /// <param name="name">The application name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public ConsoleAppBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the application version.
    /// </summary>
    /// <param name="version">The application version.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="version"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public ConsoleAppBuilder WithVersion(string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        _version = version;
        return this;
    }

    /// <summary>
    /// Sets the service provider used to resolve commands.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// When set, commands are resolved with <see cref="IServiceProvider.GetService(Type)"/>
    /// instead of being constructed directly.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public ConsoleAppBuilder UseServiceProvider(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
        return this;
    }

    /// <summary>
    /// Registers a synchronous command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the command is resolved
    /// from it; otherwise, the command is created with its public parameterless constructor.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public ConsoleAppBuilder AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _commands[name] = (serviceProvider, context) =>
            Task.FromResult(ResolveCommand<TCommand>(serviceProvider).Execute(context));
        _commandInfos[name] = new CommandInfo(name, description);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the command is resolved
    /// from it; otherwise, the command is created with its public parameterless constructor.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public ConsoleAppBuilder AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : class, IAsyncCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _commands[name] = (serviceProvider, context) => ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context);
        _commandInfos[name] = new CommandInfo(name, description);
        return this;
    }

    /// <summary>
    /// Registers the help provider used by <c>-h</c> and <c>--help</c>.
    /// </summary>
    /// <typeparam name="THelpProvider">The help provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// No default help provider is supplied. A provider must be registered to enable help.
    /// </remarks>
    public ConsoleAppBuilder AddHelp<THelpProvider>()
        where THelpProvider : class, IHelpProvider, new()
    {
        _helpProvider = new THelpProvider();
        return this;
    }

    /// <summary>
    /// Creates the configured application.
    /// </summary>
    /// <returns>A new <see cref="ConsoleApp"/> instance.</returns>
    public ConsoleApp Build() =>
        new(
            _name,
            _version,
            new Dictionary<string, Func<IServiceProvider?, CommandContext, Task<int>>>(_commands, StringComparer.Ordinal),
            [.. _commandInfos.Values],
            _serviceProvider,
            _helpProvider);

    private static TCommand ResolveCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        IServiceProvider? serviceProvider)
        where TCommand : class
    {
        return serviceProvider is null
            ? Activator.CreateInstance<TCommand>()
                ?? throw new InvalidOperationException($"Could not create command '{typeof(TCommand).FullName}'.")
            : serviceProvider.GetService(typeof(TCommand)) as TCommand
                ?? throw new InvalidOperationException($"Service '{typeof(TCommand).FullName}' is not registered.");
    }
}
