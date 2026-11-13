// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Commands.Registration;

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

    /// <inheritdoc/>
    public override ConfiguredConsoleAppBuilder<TExitCode> ConfigureErrorHandling(
        Action<CommandErrorHandlingBuilder> configure)
    {
        base.ConfigureErrorHandling(configure);
        return this;
    }
}
