// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Finishes configuring a <see cref="ConsoleApp"/> after commands have been configured.
/// </summary>
public class ConfiguredConsoleAppBuilder
{
    private readonly ConsoleAppBuilder _builder;

    internal ConfiguredConsoleAppBuilder(ConsoleAppBuilder builder)
    {
        _builder = builder;
    }

    internal ConsoleAppBuilder Builder => _builder;

    /// <summary>
    /// Sets the application name.
    /// </summary>
    /// <param name="name">The application name.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public virtual ConfiguredConsoleAppBuilder WithName(string name)
    {
        _builder.WithName(name);
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
    public virtual ConfiguredConsoleAppBuilder WithVersion(string version)
    {
        _builder.WithVersion(version);
        return this;
    }

    /// <summary>
    /// Sets the service provider used to resolve commands.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public virtual ConfiguredConsoleAppBuilder UseServiceProvider(IServiceProvider serviceProvider)
    {
        _builder.UseServiceProvider(serviceProvider);
        return this;
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
    public virtual ConfiguredConsoleAppBuilder AddInterceptor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TInterceptor>()
        where TInterceptor : class, ICommandInterceptor
    {
        _builder.AddInterceptor<TInterceptor>();
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
    public virtual ConfiguredConsoleAppBuilder AddHelp<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THelpProvider>()
        where THelpProvider : class, IHelpProvider
    {
        _builder.AddHelp<THelpProvider>();
        return this;
    }

    /// <summary>
    /// Registers the handler used for command-line errors.
    /// </summary>
    /// <typeparam name="THandler">The error handler type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// The handler is resolved from the service provider when one is configured; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public virtual ConfiguredConsoleAppBuilder UseErrorHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>()
        where THandler : class, ICommandErrorHandler
    {
        _builder.UseErrorHandler<THandler>();
        return this;
    }

    /// <summary>
    /// Registers the provider used to create command-line error messages.
    /// </summary>
    /// <typeparam name="TProvider">The message provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// The provider is resolved from the service provider when one is configured; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public virtual ConfiguredConsoleAppBuilder UseErrorMessageProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, ICommandErrorMessageProvider
    {
        _builder.UseErrorMessageProvider<TProvider>();
        return this;
    }

    /// <summary>
    /// Registers the writer used for command-line error messages.
    /// </summary>
    /// <typeparam name="TWriter">The error writer type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// The writer is resolved from the service provider when one is configured; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public virtual ConfiguredConsoleAppBuilder UseErrorWriter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWriter>()
        where TWriter : class, ICommandErrorWriter
    {
        _builder.UseErrorWriter<TWriter>();
        return this;
    }

    /// <summary>
    /// Creates the configured application.
    /// </summary>
    /// <returns>A new <see cref="ConsoleApp"/> instance.</returns>
    public ConsoleApp Build() => _builder.Build();
}
