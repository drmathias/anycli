// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
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
    private readonly GlobalOptionBuilder _globalOptions = new();
    private Func<IServiceProvider?, IHelpProvider>? _helpProviderFactory;
    private IServiceProvider? _serviceProvider;
    private Func<IServiceProvider?, IUnrecognizedCommandHandler> _unrecognizedCommandHandlerFactory =
        static _ => new DefaultUnrecognizedCommandHandler();
    private int _unrecognizedCommandExitCode = 2;
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
    /// Configures options that apply to every command.
    /// </summary>
    /// <param name="configure">The global option configuration callback.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public ConsoleAppBuilder WithGlobalOptions(Action<GlobalOptionBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure.Invoke(_globalOptions);
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
    /// Registers a synchronous command that does not use settings.
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
    internal ConsoleAppBuilder AddEmptyCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _commands[name] = new CommandRegistration(
            (serviceProvider, context, _) =>
                Task.FromResult(ResolveCommand<TCommand>(serviceProvider).Execute(context)),
            [],
            []);
        _commandInfos[name] = CreateCommandInfo(name, description, [], []);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that does not use settings.
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
    internal ConsoleAppBuilder AddEmptyAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : EmptyAsyncCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _commands[name] = new CommandRegistration(
            (serviceProvider, context, _) => ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context),
            [],
            []);
        _commandInfos[name] = CreateCommandInfo(name, description, [], []);
        return this;
    }

    /// <summary>
    /// Registers a synchronous command that does not use settings and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddEmptyExitCodeCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TExitCode>(string name, string description = "")
        where TCommand : EmptyCommand<TExitCode>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _commands[name] = new CommandRegistration(
            (serviceProvider, context, _) => Task.FromResult(
                Convert.ToInt32(
                    ResolveCommand<TCommand>(serviceProvider).Execute(context),
                    CultureInfo.InvariantCulture)),
            [],
            []);
        _commandInfos[name] = CreateCommandInfo(name, description, [], []);
        return this;
    }

    /// <summary>
    /// Registers an asynchronous command that does not use settings and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConsoleAppBuilder AddEmptyExitCodeAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TExitCode>(string name, string description = "")
        where TCommand : EmptyAsyncCommand<TExitCode>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _commands[name] = new CommandRegistration(
            (serviceProvider, context, _) => ConvertExitCodeAsync(
                ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context)),
            [],
            []);
        _commandInfos[name] = CreateCommandInfo(name, description, [], []);
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

        var definitions = BuildSettingsDefinitions<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context, commandArguments) =>
            {
                var settings = TSettings.Bind(commandArguments);
                return Task.FromResult(
                    ResolveCommand<TCommand>(serviceProvider).Execute(context, settings));
            },
            definitions.Arguments,
            definitions.Options);
        _commandInfos[name] = CreateCommandInfo(
            name,
            description,
            definitions.Arguments,
            definitions.Options);
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

        var definitions = BuildSettingsDefinitions<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context, commandArguments) =>
            {
                var settings = TSettings.Bind(commandArguments);
                var exitCode = ResolveCommand<TCommand>(serviceProvider).Execute(context, settings);
                return Task.FromResult(Convert.ToInt32(exitCode, CultureInfo.InvariantCulture));
            },
            definitions.Arguments,
            definitions.Options);
        _commandInfos[name] = CreateCommandInfo(
            name,
            description,
            definitions.Arguments,
            definitions.Options);
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

        var definitions = BuildSettingsDefinitions<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context, commandArguments) =>
            {
                var settings = TSettings.Bind(commandArguments);
                return ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, settings);
            },
            definitions.Arguments,
            definitions.Options);
        _commandInfos[name] = CreateCommandInfo(
            name,
            description,
            definitions.Arguments,
            definitions.Options);
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

        var definitions = BuildSettingsDefinitions<TSettings>();
        _commands[name] = new CommandRegistration(
            (serviceProvider, context, commandArguments) =>
            {
                var settings = TSettings.Bind(commandArguments);
                return ConvertExitCodeAsync(
                    ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, settings));
            },
            definitions.Arguments,
            definitions.Options);
        _commandInfos[name] = CreateCommandInfo(
            name,
            description,
            definitions.Arguments,
            definitions.Options);
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
    public ConfiguredConsoleAppBuilder<TExitCode> ConfigureCommands<TExitCode>(
        Action<CommandConfigurator<TExitCode>> configure)
        where TExitCode : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandConfigurator<TExitCode>(this));
        return new ConfiguredConsoleAppBuilder<TExitCode>(this);
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

    internal ConsoleAppBuilder UseUnrecognizedCommandHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>(
        int exitCode)
        where THandler : class, IUnrecognizedCommandHandler
    {
        _unrecognizedCommandHandlerFactory =
            serviceProvider => ResolveUnrecognizedCommandHandler<THandler>(serviceProvider);
        _unrecognizedCommandExitCode = exitCode;
        return this;
    }

    /// <summary>
    /// Creates the configured application.
    /// </summary>
    /// <returns>A new <see cref="ConsoleApp"/> instance.</returns>
    public ConsoleApp Build()
    {
        var globalOptions = _globalOptions.Build();
        ValidateGlobalOptions(globalOptions);

        return new ConsoleApp(
            _name,
            _version,
            new Dictionary<string, CommandRegistration>(_commands, StringComparer.Ordinal),
            [.. _commandInfos.Values],
            globalOptions,
            _serviceProvider,
            _helpProviderFactory,
            _unrecognizedCommandHandlerFactory,
            _unrecognizedCommandExitCode);
    }

    private void ValidateGlobalOptions(IReadOnlyList<OptionDefinition> globalOptions)
    {
        if (globalOptions.Count == 0)
        {
            return;
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var option in globalOptions)
        {
            foreach (var name in option.Names)
            {
                names.Add(name);

                if (_helpProviderFactory is not null && (name is "-h" or "--help"))
                {
                    throw new InvalidOperationException(
                        $"Global option '{name}' conflicts with a built-in help option.");
                }

                if (_version is not null && (name is "-v" or "--version"))
                {
                    throw new InvalidOperationException(
                        $"Global option '{name}' conflicts with a built-in version option.");
                }
            }
        }

        foreach (var command in _commands)
        {
            foreach (var option in command.Value.Options)
            {
                foreach (var name in option.Names)
                {
                    if (names.Contains(name))
                    {
                        throw new InvalidOperationException(
                            $"Global option '{name}' conflicts with an option on command '{command.Key}'.");
                    }
                }
            }
        }
    }

    private static SettingsDefinitions BuildSettingsDefinitions<TSettings>()
        where TSettings : class, ICommandSettings<TSettings>
    {
        var builder = new CommandSettingsBuilder();
        TSettings.Configure(builder);
        return new SettingsDefinitions(builder.Arguments.Build(), builder.Options.Build());
    }

    private static CommandInfo CreateCommandInfo(
        string name,
        string description,
        IReadOnlyList<ArgumentDefinition> arguments,
        IReadOnlyList<OptionDefinition> options) =>
        new(name, description)
        {
            Arguments = [.. arguments
                .Select(argument => new ArgumentInfo(
                    argument.Name,
                    argument.Description,
                    argument.ArgumentType,
                    argument.Arity))],
            Options = [.. options
                .Select(option => new OptionInfo(
                    option.Names,
                    option.Description,
                    option.Required,
                    option.TakesValue,
                    option.DeclaredType))],
        };

    private sealed record SettingsDefinitions(
        IReadOnlyList<ArgumentDefinition> Arguments,
        IReadOnlyList<OptionDefinition> Options);

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

    private static THandler ResolveUnrecognizedCommandHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>(
        IServiceProvider? serviceProvider)
        where THandler : class, IUnrecognizedCommandHandler
    {
        return serviceProvider is null
            ? Activator.CreateInstance<THandler>()
                ?? throw new InvalidOperationException(
                    $"Could not create unrecognized command handler '{typeof(THandler).FullName}'.")
            : serviceProvider.GetService(typeof(THandler)) as THandler
                ?? throw new InvalidOperationException(
                    $"Service '{typeof(THandler).FullName}' is not registered.");
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
