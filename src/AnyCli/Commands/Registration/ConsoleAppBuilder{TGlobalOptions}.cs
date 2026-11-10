// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Configures and creates a <see cref="ConsoleApp"/> with typed global options.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
public sealed class ConsoleAppBuilder<TGlobalOptions>
    where TGlobalOptions : class, IGlobalOptions<TGlobalOptions>
{
    private readonly ConsoleAppBuilder _builder;

    internal ConsoleAppBuilder(ConsoleAppBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Sets the application name.
    /// </summary>
    /// <param name="name">The application name.</param>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> WithName(string name)
    {
        _builder.WithName(name);
        return this;
    }

    /// <summary>
    /// Sets the application version.
    /// </summary>
    /// <param name="version">The application version.</param>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> WithVersion(string version)
    {
        _builder.WithVersion(version);
        return this;
    }

    /// <summary>
    /// Sets the service provider used to resolve commands.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> UseServiceProvider(IServiceProvider serviceProvider)
    {
        _builder.UseServiceProvider(serviceProvider);
        return this;
    }

    /// <summary>
    /// Registers a command execution interceptor.
    /// </summary>
    /// <typeparam name="TInterceptor">The interceptor type.</typeparam>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> AddInterceptor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TInterceptor>()
        where TInterceptor : class, ICommandInterceptor<TGlobalOptions>
    {
        _builder.AddInterceptor<TInterceptor>();
        return this;
    }

    /// <summary>
    /// Registers the help provider used by <c>-h</c> and <c>--help</c>.
    /// </summary>
    /// <typeparam name="THelpProvider">The help provider type.</typeparam>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> AddHelp<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THelpProvider>()
        where THelpProvider : class, IHelpProvider
    {
        _builder.AddHelp<THelpProvider>();
        return this;
    }

    /// <summary>
    /// Configures the handler and exit code used when the command is not recognized.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="exitCode">The exit code returned for an unrecognized command.</param>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> UseUnrecognizedCommandHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>(
        int exitCode = 2)
        where THandler : class, IUnrecognizedCommandHandler
    {
        _builder.UseUnrecognizedCommandHandler<THandler>(exitCode);
        return this;
    }

    /// <summary>
    /// Configures commands that return standard integer exit codes.
    /// </summary>
    /// <param name="configure">The configuration callback.</param>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> ConfigureCommands(
        Action<CommandRegistrar<TGlobalOptions>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandRegistrar<TGlobalOptions>(_builder, _builder.Root));
        return this;
    }

    /// <summary>
    /// Configures commands that return a custom exit code enum.
    /// </summary>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="configure">The configuration callback.</param>
    /// <returns>The current builder.</returns>
    public ConsoleAppBuilder<TGlobalOptions> ConfigureCommands<TExitCode>(
        Action<CommandRegistrar<TGlobalOptions, TExitCode>> configure)
        where TExitCode : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new CommandRegistrar<TGlobalOptions, TExitCode>(_builder, _builder.Root));
        return this;
    }

    /// <summary>
    /// Creates the configured application.
    /// </summary>
    /// <returns>A new <see cref="ConsoleApp"/> instance.</returns>
    public ConsoleApp Build()
    {
        _builder.ValidateGlobalOptionsBase(typeof(TGlobalOptions));
        return _builder.Build();
    }
}
