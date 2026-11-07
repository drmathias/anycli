// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AnyCli;

/// <summary>
/// Configures and creates a <see cref="ConsoleApp"/>.
/// </summary>
public sealed class ConsoleAppBuilder
{
    private readonly Dictionary<string, CommandRegistration> _commands = new(StringComparer.Ordinal);

    private readonly Dictionary<string, CommandInfo> _commandInfos = new(StringComparer.Ordinal);
    private Func<IServiceProvider?, IHelpProvider>? _helpProviderFactory;
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
    /// <param name="configureArguments">
    /// An optional callback that defines the command's positional arguments.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the command is resolved
    /// from it; otherwise, the command is created with its public parameterless constructor.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "",
        Action<CommandArgumentBuilder>? configureArguments = null)
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var arguments = BuildArguments(configureArguments);
        _commands[name] = new CommandRegistration(
            (serviceProvider, context) =>
                Task.FromResult(ResolveCommand<TCommand>(serviceProvider).Execute(context)),
            arguments);
        _commandInfos[name] = CreateCommandInfo(name, description, arguments);
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
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the command is resolved
    /// from it; otherwise, the command is created with its public parameterless constructor.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "",
        Action<CommandArgumentBuilder>? configureArguments = null)
        where TCommand : class, IAsyncCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var arguments = BuildArguments(configureArguments);
        _commands[name] = new CommandRegistration(
            (serviceProvider, context) => ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context),
            arguments);
        _commandInfos[name] = CreateCommandInfo(name, description, arguments);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed settings generated by the AnyCli source generator.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var arguments = BuildSettingsArguments<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context) =>
            {
                var settings = TSettings.Bind(context.Arguments);
                return Task.FromResult(
                    ResolveCommand<TCommand>(serviceProvider).Execute(context, settings));
            },
            arguments);
        _commandInfos[name] = CreateCommandInfo(name, description, arguments);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed settings and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddExitCodeCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings,
        TExitCode>(string name, string description = "")
        where TCommand : class, ICommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var arguments = BuildSettingsArguments<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context) =>
            {
                var settings = TSettings.Bind(context.Arguments);
                var exitCode = ResolveCommand<TCommand>(serviceProvider).Execute(context, settings);
                return Task.FromResult(Convert.ToInt32(exitCode, CultureInfo.InvariantCulture));
            },
            arguments);
        _commandInfos[name] = CreateCommandInfo(name, description, arguments);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed settings generated by the AnyCli source generator.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var arguments = BuildSettingsArguments<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context) =>
            {
                var settings = TSettings.Bind(context.Arguments);
                return ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, settings);
            },
            arguments);
        _commandInfos[name] = CreateCommandInfo(name, description, arguments);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed settings and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TSettings">The settings type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddExitCodeAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TSettings,
        TExitCode>(string name, string description = "")
        where TCommand : class, IAsyncCommand<TSettings, TExitCode>
        where TSettings : class, ICommandSettings<TSettings>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var arguments = BuildSettingsArguments<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context) =>
            {
                var settings = TSettings.Bind(context.Arguments);
                return ConvertExitCodeAsync(
                    ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, settings));
            },
            arguments);
        _commandInfos[name] = CreateCommandInfo(name, description, arguments);
        return this;
    }

    /// <summary>
    /// Configures commands that return standard integer exit codes.
    /// </summary>
    /// <param name="configure">The configuration callback.</param>
    /// <returns>A builder that can be used to finish configuring the application.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public ConfiguredConsoleAppBuilder ConfigureCommands(Action<CommandConfigurator> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandConfigurator(this));
        return new ConfiguredConsoleAppBuilder(this);
    }

    /// <summary>
    /// Configures commands that return a custom exit code enum.
    /// </summary>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="configure">The configuration callback.</param>
    /// <returns>A builder that can be used to finish configuring the application.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public ConfiguredConsoleAppBuilder ConfigureCommands<TExitCode>(Action<CommandConfigurator<TExitCode>> configure)
        where TExitCode : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandConfigurator<TExitCode>(this));
        return new ConfiguredConsoleAppBuilder(this);
    }

    /// <summary>
    /// Registers the help provider used by <c>-h</c> and <c>--help</c>.
    /// </summary>
    /// <typeparam name="THelpProvider">The help provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// No default help provider is supplied. A provider must be registered to enable help.
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the help provider is
    /// resolved from it; otherwise, it is created with its public parameterless constructor.
    /// </remarks>
    public ConsoleAppBuilder AddHelp<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THelpProvider>()
        where THelpProvider : class, IHelpProvider
    {
        _helpProviderFactory = serviceProvider => ResolveHelpProvider<THelpProvider>(serviceProvider);
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
            new Dictionary<string, CommandRegistration>(_commands, StringComparer.Ordinal),
            [.. _commandInfos.Values],
            _serviceProvider,
            _helpProviderFactory);

    private static IReadOnlyList<ArgumentDefinition> BuildArguments(
        Action<CommandArgumentBuilder>? configureArguments)
    {
        if (configureArguments is null)
        {
            return [];
        }

        var builder = new CommandArgumentBuilder();
        configureArguments(builder);
        return builder.Build();
    }

    private static IReadOnlyList<ArgumentDefinition> BuildSettingsArguments<TSettings>()
        where TSettings : class, ICommandSettings<TSettings>
    {
        var builder = new CommandArgumentBuilder();
        TSettings.Configure(builder);
        return builder.Build();
    }

    private static CommandInfo CreateCommandInfo(
        string name,
        string description,
        IReadOnlyList<ArgumentDefinition> arguments) =>
        new(name, description)
        {
            Arguments = arguments
                .Select(argument => new ArgumentInfo(
                    argument.Name,
                    argument.Description,
                    argument.Kind == ArgumentKind.Required,
                    argument.Kind == ArgumentKind.Variadic))
                .ToArray(),
        };

    private static async Task<int> ConvertExitCodeAsync<TExitCode>(Task<TExitCode> exitCode)
        where TExitCode : struct, Enum =>
        Convert.ToInt32(await exitCode.ConfigureAwait(false), CultureInfo.InvariantCulture);

    private static THelpProvider ResolveHelpProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THelpProvider>(
        IServiceProvider? serviceProvider)
        where THelpProvider : class
    {
        return serviceProvider is null
            ? Activator.CreateInstance<THelpProvider>()
                ?? throw new InvalidOperationException(
                    $"Could not create help provider '{typeof(THelpProvider).FullName}'.")
            : serviceProvider.GetService(typeof(THelpProvider)) as THelpProvider
                ?? throw new InvalidOperationException(
                    $"Service '{typeof(THelpProvider).FullName}' is not registered.");
    }

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
