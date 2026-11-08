// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace AnyCli;

/// <summary>
/// Finishes configuring a <see cref="ConsoleApp"/> with a custom command exit code type.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public sealed class ConfiguredConsoleAppBuilder<TExitCode> : ConfiguredConsoleAppBuilder
    where TExitCode : struct, Enum
{
    internal ConfiguredConsoleAppBuilder(ConsoleAppBuilder builder)
        : base(builder)
    {
    }

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> WithName(string name)
    {
        base.WithName(name);
        return this;
    }

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> WithVersion(string version)
    {
        base.WithVersion(version);
        return this;
    }

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> WithGlobalOptions(
        Action<GlobalOptionBuilder> configure)
    {
        base.WithGlobalOptions(configure);
        return this;
    }

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> UseServiceProvider(
        IServiceProvider serviceProvider)
    {
        base.UseServiceProvider(serviceProvider);
        return this;
    }

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> AddInterceptor<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TInterceptor>()
        where TInterceptor : class
    {
        base.AddInterceptor<TInterceptor>();
        return this;
    }

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> AddHelp<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THelpProvider>()
        where THelpProvider : class
    {
        base.AddHelp<THelpProvider>();
        return this;
    }

    /// <summary>
    /// Configures the handler and exit code used when the command is not recognized.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="exitCode">The exit code returned for an unrecognized command.</param>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// The handler is resolved from the service provider when one is configured; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public ConfiguredConsoleAppBuilder<TExitCode> UseUnrecognizedCommandHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] THandler>(
        TExitCode exitCode)
        where THandler : class, IUnrecognizedCommandHandler
    {
        Builder.UseUnrecognizedCommandHandler<THandler>(Convert.ToInt32(exitCode, CultureInfo.InvariantCulture));
        return this;
    }
}
