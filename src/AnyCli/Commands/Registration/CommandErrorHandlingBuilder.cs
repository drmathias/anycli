// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Configures command-line error handling.
/// </summary>
public sealed class CommandErrorHandlingBuilder
{
    private readonly ConsoleAppBuilder _builder;

    internal CommandErrorHandlingBuilder(ConsoleAppBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Registers the handler used for command-line errors.
    /// </summary>
    /// <typeparam name="THandler">The error handler type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// When the handler is registered in the configured service provider, that instance is used;
    /// otherwise, a public parameterless constructor is used.
    /// </remarks>
    public CommandErrorHandlingBuilder UseHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>()
        where THandler : class, ICommandErrorHandler
    {
        _builder.UseErrorHandlerCore<THandler>();
        return this;
    }

    /// <summary>
    /// Registers the factory used to create the command-line error handler.
    /// </summary>
    /// <param name="factory">The factory that creates the error handler.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseHandler(
        Func<CommandErrorHandlerContext, ICommandErrorHandler> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _builder.UseErrorHandlerCore(factory);
        return this;
    }

    /// <summary>
    /// Registers the provider used to create command-line error messages.
    /// </summary>
    /// <typeparam name="TProvider">The message provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// When the provider is registered in the configured service provider, that instance is used;
    /// otherwise, a public parameterless constructor is used.
    /// </remarks>
    public CommandErrorHandlingBuilder UseMessageProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, ICommandErrorMessageProvider
    {
        _builder.UseErrorMessageProviderCore<TProvider>();
        return this;
    }

    /// <summary>
    /// Uses the specified command-line error message provider.
    /// </summary>
    /// <param name="provider">The message provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseMessageProvider(ICommandErrorMessageProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _builder.UseErrorMessageProviderCore(provider);
        return this;
    }

    /// <summary>
    /// Uses the localized error message provider with the specified message resources.
    /// </summary>
    /// <param name="messageResources">
    /// The resource managers used to resolve built-in message keys, in priority order.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="messageResources"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseLocalizedMessageProvider(
        params ResourceManager[] messageResources)
    {
        ArgumentNullException.ThrowIfNull(messageResources);
        _builder.UseLocalizedErrorMessageProviderCore(messageResources);
        return this;
    }

    /// <summary>
    /// Registers the writer used for command-line error messages.
    /// </summary>
    /// <typeparam name="TWriter">The error writer type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// When the writer is registered in the configured service provider, that instance is used;
    /// otherwise, a public parameterless constructor is used.
    /// </remarks>
    public CommandErrorHandlingBuilder UseWriter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWriter>()
        where TWriter : class, ICommandErrorWriter
    {
        _builder.UseErrorWriterCore<TWriter>();
        return this;
    }

    /// <summary>
    /// Uses the specified command-line error writer.
    /// </summary>
    /// <param name="writer">The error writer.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="writer"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseWriter(ICommandErrorWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        _builder.UseErrorWriterCore(writer);
        return this;
    }

    /// <summary>
    /// Sets the exit code returned after a command-line error is handled.
    /// </summary>
    /// <param name="exitCode">The process exit code.</param>
    /// <returns>The current builder.</returns>
    public CommandErrorHandlingBuilder UseExitCode(int exitCode)
    {
        _builder.SetErrorExitCodeProvider(_ => exitCode);
        return this;
    }

    /// <summary>
    /// Sets the exit code returned after a command-line error is handled.
    /// </summary>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="exitCode">The process exit code.</param>
    /// <returns>The current builder.</returns>
    public CommandErrorHandlingBuilder UseExitCode<TExitCode>(TExitCode exitCode)
        where TExitCode : struct, Enum =>
        UseExitCode(Convert.ToInt32(exitCode, CultureInfo.InvariantCulture));

    /// <summary>
    /// Sets the provider used to choose a typed exit code after a command-line error is handled.
    /// </summary>
    /// <typeparam name="TExitCode">The exit code enum type.</typeparam>
    /// <param name="exitCodeProvider">The exit code provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exitCodeProvider"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseExitCode<TExitCode>(
        Func<CommandError, TExitCode> exitCodeProvider)
        where TExitCode : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(exitCodeProvider);
        _builder.SetErrorExitCodeProvider(error =>
            Convert.ToInt32(exitCodeProvider(error), CultureInfo.InvariantCulture));
        return this;
    }

    /// <summary>
    /// Sets the provider used to choose the exit code after a command-line error is handled.
    /// </summary>
    /// <param name="exitCodeProvider">The exit code provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exitCodeProvider"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseExitCode(
        Func<CommandError, int> exitCodeProvider)
    {
        ArgumentNullException.ThrowIfNull(exitCodeProvider);
        _builder.SetErrorExitCodeProvider(exitCodeProvider);
        return this;
    }
}
