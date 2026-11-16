// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
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
    /// Uses the localized error message provider with resources created by a factory.
    /// </summary>
    /// <param name="messageResourcesFactory">
    /// The factory that creates message resource managers, in priority order.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="messageResourcesFactory"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseLocalizedMessageProvider(
        Func<IServiceProvider?, IEnumerable<ResourceManager>> messageResourcesFactory)
    {
        ArgumentNullException.ThrowIfNull(messageResourcesFactory);
        _builder.UseLocalizedErrorMessageProviderCore(messageResourcesFactory);
        return this;
    }

    /// <summary>
    /// Uses the localized error message provider with the specified message resources.
    /// </summary>
    /// <param name="baseName">The resource base name.</param>
    /// <param name="assembly">The assembly containing the resources.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="baseName"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="assembly"/> is <see langword="null"/>.
    /// </exception>
    public CommandErrorHandlingBuilder UseLocalizedMessageProvider(
        string baseName,
        Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);
        ArgumentNullException.ThrowIfNull(assembly);

        var resourceManager = new ResourceManager(baseName, assembly);
        return UseLocalizedMessageProvider(resourceManager);
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
