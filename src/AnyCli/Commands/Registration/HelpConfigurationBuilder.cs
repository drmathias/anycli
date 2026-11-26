// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;

namespace AnyCli.Commands.Registration;

/// <summary>
/// Configures command-line help output.
/// </summary>
public sealed class HelpConfigurationBuilder
{
    private readonly ConsoleAppBuilder _builder;

    internal HelpConfigurationBuilder(ConsoleAppBuilder builder)
    {
        _builder = builder;
    }

    /// <summary>
    /// Uses the specified help text provider type.
    /// </summary>
    /// <typeparam name="TProvider">The help text provider type.</typeparam>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseTextProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, IHelpTextProvider
    {
        _builder.HelpFormatting.UseTextProvider<TProvider>();
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with the specified description resources.
    /// </summary>
    /// <param name="descriptionResources">The resource managers used to resolve descriptions.</param>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseLocalizedTextProvider(
        params ResourceManager[] descriptionResources)
    {
        _builder.HelpFormatting.UseLocalizedTextProvider(descriptionResources);
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with resources created by a factory.
    /// </summary>
    /// <param name="descriptionResourcesFactory">The resource manager factory.</param>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseLocalizedTextProvider(
        Func<IServiceProvider?, IEnumerable<ResourceManager>> descriptionResourcesFactory)
    {
        _builder.HelpFormatting.UseLocalizedTextProvider(descriptionResourcesFactory);
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with resources from the specified assembly.
    /// </summary>
    /// <param name="baseName">The resource base name.</param>
    /// <param name="assembly">The assembly containing the resources.</param>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseLocalizedTextProvider(
        string baseName,
        Assembly assembly)
    {
        _builder.HelpFormatting.UseLocalizedTextProvider(baseName, assembly);
        return this;
    }

    /// <summary>
    /// Uses the specified default value formatter type.
    /// </summary>
    /// <typeparam name="TFormatter">The formatter type.</typeparam>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseValueFormatter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFormatter>()
        where TFormatter : class, IHelpValueFormatter
    {
        _builder.HelpFormatting.UseValueFormatter<TFormatter>();
        return this;
    }

    /// <summary>
    /// Uses the specified default value formatter.
    /// </summary>
    /// <param name="formatter">The formatter.</param>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseValueFormatter(IHelpValueFormatter formatter)
    {
        _builder.HelpFormatting.UseValueFormatter(formatter);
        return this;
    }

    /// <summary>
    /// Uses a factory to create the default value formatter.
    /// </summary>
    /// <param name="factory">The formatter factory.</param>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseValueFormatter(
        Func<IServiceProvider?, IHelpValueFormatter> factory)
    {
        _builder.HelpFormatting.UseValueFormatter(factory);
        return this;
    }

    /// <summary>
    /// Registers the provider used to render help output.
    /// </summary>
    /// <typeparam name="TProvider">The help provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was configured, the provider is resolved from it; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public HelpConfigurationBuilder UseProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, IHelpProvider
    {
        _builder.UseHelpProviderCore<TProvider>();
        return this;
    }

    /// <summary>
    /// Registers the factory used to create the help provider.
    /// </summary>
    /// <param name="factory">The factory that creates the help provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public HelpConfigurationBuilder UseProvider(
        Func<HelpProviderContext, IHelpProvider> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _builder.UseHelpProviderCore(factory);
        return this;
    }
}
