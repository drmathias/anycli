// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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
    /// If a service provider was configured, the handler is resolved from it; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public CommandErrorHandlingBuilder UseHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>()
        where THandler : class, ICommandErrorHandler
    {
        _builder.UseErrorHandlerCore<THandler>();
        return this;
    }

    /// <summary>
    /// Registers the provider used to create command-line error messages.
    /// </summary>
    /// <typeparam name="TProvider">The message provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was configured, the provider is resolved from it; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public CommandErrorHandlingBuilder UseMessageProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, ICommandErrorMessageProvider
    {
        _builder.UseErrorMessageProviderCore<TProvider>();
        return this;
    }

    /// <summary>
    /// Registers the writer used for command-line error messages.
    /// </summary>
    /// <typeparam name="TWriter">The error writer type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was configured, the writer is resolved from it; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public CommandErrorHandlingBuilder UseWriter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TWriter>()
        where TWriter : class, ICommandErrorWriter
    {
        _builder.UseErrorWriterCore<TWriter>();
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
    /// Sets the provider used to choose the exit code after a command-line error is handled.
    /// </summary>
    /// <param name="exitCodeProvider">The exit code provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="exitCodeProvider"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseExitCode(
        Func<CommandErrorContext, int> exitCodeProvider)
    {
        ArgumentNullException.ThrowIfNull(exitCodeProvider);
        _builder.SetErrorExitCodeProvider(exitCodeProvider);
        return this;
    }
}
