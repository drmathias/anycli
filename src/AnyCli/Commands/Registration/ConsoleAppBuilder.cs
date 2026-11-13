// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Configures and creates a <see cref="ConsoleApp"/>.
/// </summary>
public sealed class ConsoleAppBuilder
{
    private readonly GlobalOptionBuilder _globalOptions = new();
    private readonly List<Func<IServiceProvider?, ICommandInterceptor>> _interceptorFactories = [];
    private Func<IServiceProvider?, ICommandErrorHandler>? _errorHandlerFactory;
    private Func<IServiceProvider?, ICommandErrorMessageProvider> _errorMessageProviderFactory =
        static _ => new LocalizedCommandErrorMessageProvider();
    private Func<IServiceProvider?, ICommandErrorWriter> _errorWriterFactory =
        static _ => new PlainCommandErrorWriter();
    private IGlobalOptionsDefinition? _globalOptionsDefinition;
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
    /// Configures typed global options that apply to every command.
    /// </summary>
    /// <typeparam name="TGlobalOptions">The global options type.</typeparam>
    /// <returns>A builder that registers commands using <typeparamref name="TGlobalOptions"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Global options have already been configured.
    /// </exception>
    public ConsoleAppBuilder<TGlobalOptions> AddGlobalOptions<TGlobalOptions>()
        where TGlobalOptions : class, IGlobalOptions<TGlobalOptions>
    {
        if (_globalOptionsDefinition is not null)
        {
            throw new InvalidOperationException(
                "Global options have already been configured.");
        }

        TGlobalOptions.Configure(_globalOptions);
        _globalOptionsDefinition = new GlobalOptionsDefinition<TGlobalOptions>();

        return new ConsoleAppBuilder<TGlobalOptions>(this);
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
    /// Registers a synchronous command that does not use parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="parent">The parent command group.</param>
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
    internal CommandNode AddNoParametersCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        CommandNode parent, string name,
        string description = "")
        where TCommand : NoParametersCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var registration = new CommandRegistration(
            (serviceProvider, context, _, _, _) =>
                Task.FromResult(ResolveCommand<TCommand>(serviceProvider).Execute(context)),
            [],
            []);
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers an asynchronous command that does not use parameters.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <param name="parent">The parent command group.</param>
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
    internal CommandNode AddNoParametersAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        CommandNode parent, string name,
        string description = "")
        where TCommand : NoParametersAsyncCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var registration = new CommandRegistration(
            (serviceProvider, context, _, _, cancellationToken) =>
                ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, cancellationToken),
            [],
            []);
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers a synchronous command that does not use parameters and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="parent">The parent command group.</param>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal CommandNode AddNoParametersExitCodeCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TExitCode>(CommandNode parent, string name, string description = "")
        where TCommand : NoParametersCommand<TExitCode>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var registration = new CommandRegistration(
            (serviceProvider, context, _, _, _) => Task.FromResult(
                Convert.ToInt32(
                    ResolveCommand<TCommand>(serviceProvider).Execute(context),
                    CultureInfo.InvariantCulture)),
            [],
            []);
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers an asynchronous command that does not use parameters and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="parent">The parent command group.</param>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal CommandNode AddNoParametersExitCodeAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TExitCode>(CommandNode parent, string name, string description = "")
        where TCommand : NoParametersAsyncCommand<TExitCode>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var registration = new CommandRegistration(
            (serviceProvider, context, _, _, cancellationToken) => ConvertExitCodeAsync(
                ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, cancellationToken)),
            [],
            []);
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed parameters generated by the AnyCli source generator.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="parent">The parent command group.</param>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal CommandNode AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(CommandNode parent, string name, string description = "")
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, ICommandParameters<TParameters>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definitions = BuildParametersDefinitions<TParameters>();
        var registration = new CommandRegistration(
            (serviceProvider, context, commandArguments, globalOptionValues, _) =>
            {
                var parameters = TParameters.Bind(commandArguments, globalOptionValues);
                return Task.FromResult(
                    ResolveCommand<TCommand>(serviceProvider).Execute(context, parameters));
            },
            definitions.Arguments,
            definitions.Options)
        {
            ParametersType = typeof(TParameters),
        };
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers a synchronous command that uses typed parameters and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="parent">The parent command group.</param>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal CommandNode AddExitCodeCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters,
        TExitCode>(CommandNode parent, string name, string description = "")
        where TCommand : class, ICommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definitions = BuildParametersDefinitions<TParameters>();
        var registration = new CommandRegistration(
            (serviceProvider, context, commandArguments, globalOptionValues, _) =>
            {
                var parameters = TParameters.Bind(commandArguments, globalOptionValues);
                var exitCode = ResolveCommand<TCommand>(serviceProvider).Execute(context, parameters);
                return Task.FromResult(Convert.ToInt32(exitCode, CultureInfo.InvariantCulture));
            },
            definitions.Arguments,
            definitions.Options)
        {
            ParametersType = typeof(TParameters),
        };
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed parameters generated by the AnyCli source generator.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <param name="parent">The parent command group.</param>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal CommandNode AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters>(CommandNode parent, string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters>
        where TParameters : class, ICommandParameters<TParameters>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definitions = BuildParametersDefinitions<TParameters>();
        var registration = new CommandRegistration(
            (serviceProvider, context, commandArguments, globalOptionValues, cancellationToken) =>
            {
                var parameters = TParameters.Bind(commandArguments, globalOptionValues);
                return ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, parameters, cancellationToken);
            },
            definitions.Arguments,
            definitions.Options)
        {
            ParametersType = typeof(TParameters),
        };
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Registers an asynchronous command that uses typed parameters and returns a custom exit code enum.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <typeparam name="TParameters">The parameters type.</typeparam>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="parent">The parent command group.</param>
    /// <param name="name">The command name.</param>
    /// <param name="description">A short description shown in help output.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal CommandNode AddExitCodeAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand,
        TParameters,
        TExitCode>(CommandNode parent, string name, string description = "")
        where TCommand : class, IAsyncCommand<TParameters, TExitCode>
        where TParameters : class, ICommandParameters<TParameters>
        where TExitCode : struct, Enum
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definitions = BuildParametersDefinitions<TParameters>();
        var registration = new CommandRegistration(
            (serviceProvider, context, commandArguments, globalOptionValues, cancellationToken) =>
            {
                var parameters = TParameters.Bind(commandArguments, globalOptionValues);
                return ConvertExitCodeAsync(
                    ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context, parameters, cancellationToken));
            },
            definitions.Arguments,
            definitions.Options)
        {
            ParametersType = typeof(TParameters),
        };
        var command = new CommandNode(name, description) { Command = registration };
        parent.AddChild(command);
        return command;
    }

    /// <summary>
    /// Configures commands that return standard integer exit codes.
    /// </summary>
    /// <param name="configure">The configuration callback.</param>
    /// <returns>A builder that can be used to finish configuring the application.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="configure"/> is <see langword="null"/>.
    /// </exception>
    public ConfiguredConsoleAppBuilder ConfigureCommands(Action<CommandRegistrar> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandRegistrar(this, Root));
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
        Action<CommandRegistrarWithExitCode<TExitCode>> configure)
        where TExitCode : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandRegistrarWithExitCode<TExitCode>(this, Root));
        return new ConfiguredConsoleAppBuilder<TExitCode>(this);
    }

    /// <summary>
    /// Registers a command execution interceptor.
    /// </summary>
    /// <typeparam name="TInterceptor">The interceptor type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// Interceptors run in registration order before the command and in reverse order after it.
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the interceptor is
    /// resolved from it; otherwise, it is created with its public parameterless constructor.
    /// </remarks>
    public ConsoleAppBuilder AddInterceptor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TInterceptor>()
        where TInterceptor : class, ICommandInterceptor
    {
        _interceptorFactories.Add(serviceProvider => ResolveInterceptor<TInterceptor>(serviceProvider));
        return this;
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
    /// Registers the handler used for command-line errors.
    /// </summary>
    /// <typeparam name="THandler">The error handler type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the handler is
    /// resolved from it; otherwise, it is created with its public parameterless constructor.
    /// </remarks>
    public ConsoleAppBuilder UseErrorHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>()
        where THandler : class, ICommandErrorHandler
    {
        _errorHandlerFactory = ResolveErrorHandler<THandler>;
        return this;
    }

    /// <summary>
    /// Registers the provider used to create command-line error messages.
    /// </summary>
    /// <typeparam name="TProvider">The message provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the provider is
    /// resolved from it; otherwise, it is created with its public parameterless constructor.
    /// </remarks>
    public ConsoleAppBuilder UseErrorMessageProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, ICommandErrorMessageProvider
    {
        _errorMessageProviderFactory = ResolveErrorMessageProvider<TProvider>;
        return this;
    }

    /// <summary>
    /// Registers the writer used for command-line error messages.
    /// </summary>
    /// <typeparam name="TWriter">The error writer type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was set with <see cref="UseServiceProvider"/>, the writer is
    /// resolved from it; otherwise, it is created with its public parameterless constructor.
    /// </remarks>
    public ConsoleAppBuilder UseErrorWriter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWriter>()
        where TWriter : class, ICommandErrorWriter
    {
        _errorWriterFactory = ResolveErrorWriter<TWriter>;
        return this;
    }

    internal void ValidateGlobalOptionsBase(Type globalOptionsType)
    {
        foreach (var (commandName, command) in EnumerateCommands(Root, string.Empty))
        {
            if (command.ParametersType is null)
            {
                throw new InvalidOperationException(
                    $"Command '{commandName}' must declare parameters that derive from '{globalOptionsType.FullName}'.");
            }

            if (!globalOptionsType.IsAssignableFrom(command.ParametersType))
            {
                throw new InvalidOperationException(
                    $"Command '{commandName}' parameters type '{command.ParametersType.FullName}' must derive from '{globalOptionsType.FullName}'.");
            }
        }
    }

    /// <summary>
    /// Creates the configured application.
    /// </summary>
    /// <returns>A new <see cref="ConsoleApp"/> instance.</returns>
    public ConsoleApp Build()
    {
        ValidateCommandTree(Root);

        var globalOptions = _globalOptions.Build();
        ValidateGlobalOptions(globalOptions);

        return new ConsoleApp(
            _name,
            _version,
            Root,
            globalOptions,
            _globalOptionsDefinition,
            _serviceProvider,
            _helpProviderFactory,
            _errorHandlerFactory ?? CreateDefaultErrorHandlerFactory(),
            [.. _interceptorFactories]);
    }

    private Func<IServiceProvider?, ICommandErrorHandler> CreateDefaultErrorHandlerFactory() =>
        serviceProvider => new DefaultCommandErrorHandler(
            _errorMessageProviderFactory(serviceProvider),
            _errorWriterFactory(serviceProvider));

    private static void ValidateCommandTree(CommandNode parent)
    {
        foreach (var child in parent.Children)
        {
            if (child.Children.Count > 0)
            {
                EnsureCanHaveSubcommands(child);
            }

            ValidateCommandTree(child);
        }
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

        foreach (var (commandName, command) in EnumerateCommands(Root, string.Empty))
        {
            foreach (var option in command.Options)
            {
                foreach (var name in option.Names)
                {
                    if (names.Contains(name))
                    {
                        throw new InvalidOperationException(
                            $"Global option '{name}' conflicts with an option on command '{commandName}'.");
                    }
                }
            }
        }
    }

    private static ParametersDefinitions BuildParametersDefinitions<TParameters>()
        where TParameters : class, ICommandParameters<TParameters>
    {
        var builder = new CommandParameterDefinitionBuilder();
        TParameters.Configure(builder);
        return new ParametersDefinitions(builder.Arguments.Build(), builder.Options.Build());
    }

    internal CommandNode Root { get; } = new(string.Empty, string.Empty);

    internal static void EnsureCanHaveSubcommands(CommandNode command)
    {
        if (command.Command is { Arguments.Count: > 0 })
        {
            throw new InvalidOperationException(
                $"Command '{command.Name}' has subcommands and cannot define positional arguments.");
        }
    }

    internal CommandNode AddGroup(CommandNode parent, string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var group = new CommandNode(name, description);
        parent.AddChild(group);
        return group;
    }

    private static IEnumerable<(string Name, CommandRegistration Command)> EnumerateCommands(
        CommandNode parent,
        string prefix)
    {
        foreach (var child in parent.Children)
        {
            var name = prefix.Length == 0 ? child.Name : prefix + " " + child.Name;
            if (child.Command is { } command)
            {
                yield return (name, command);
            }

            foreach (var nested in EnumerateCommands(child, name))
            {
                yield return nested;
            }
        }
    }

    private sealed record ParametersDefinitions(
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

    private static THandler ResolveErrorHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>(
        IServiceProvider? serviceProvider)
        where THandler : class, ICommandErrorHandler =>
        ResolveErrorService<THandler>(serviceProvider, "command error handler");

    private static TProvider ResolveErrorMessageProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>(
        IServiceProvider? serviceProvider)
        where TProvider : class, ICommandErrorMessageProvider =>
        ResolveErrorService<TProvider>(serviceProvider, "command error message provider");

    private static TWriter ResolveErrorWriter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWriter>(
        IServiceProvider? serviceProvider)
        where TWriter : class, ICommandErrorWriter =>
        ResolveErrorService<TWriter>(serviceProvider, "command error writer");

    private static TService ResolveErrorService<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TService>(
        IServiceProvider? serviceProvider,
        string serviceDescription)
        where TService : class
    {
        return serviceProvider is null
            ? Activator.CreateInstance<TService>()
                ?? throw new InvalidOperationException(
                    $"Could not create {serviceDescription} '{typeof(TService).FullName}'.")
            : serviceProvider.GetService(typeof(TService)) as TService
                ?? throw new InvalidOperationException(
                    $"Service '{typeof(TService).FullName}' is not registered.");
    }

    private static TInterceptor ResolveInterceptor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TInterceptor>(
        IServiceProvider? serviceProvider)
        where TInterceptor : class
    {
        return serviceProvider is null
            ? Activator.CreateInstance<TInterceptor>()
                ?? throw new InvalidOperationException(
                    $"Could not create command interceptor '{typeof(TInterceptor).FullName}'.")
            : serviceProvider.GetService(typeof(TInterceptor)) as TInterceptor
                ?? throw new InvalidOperationException(
                    $"Service '{typeof(TInterceptor).FullName}' is not registered.");
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
