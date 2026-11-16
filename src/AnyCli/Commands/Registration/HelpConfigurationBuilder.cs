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
    /// Registers the provider used for localized help text.
    /// </summary>
    /// <typeparam name="TProvider">The help text provider type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// If a service provider was configured, the provider is resolved from it; otherwise, it is
    /// created with its public parameterless constructor.
    /// </remarks>
    public HelpConfigurationBuilder UseTextProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, IHelpTextProvider
    {
        _builder.UseHelpTextProviderCore<TProvider>();
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with the specified description resources.
    /// </summary>
    /// <param name="descriptionResources">
    /// The resource managers used to resolve description resource keys, in priority order.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="descriptionResources"/> is <see langword="null"/>.
    /// </exception>
    public HelpConfigurationBuilder UseLocalizedTextProvider(
        params ResourceManager[] descriptionResources)
    {
        ArgumentNullException.ThrowIfNull(descriptionResources);
        _builder.UseLocalizedHelpTextProviderCore(descriptionResources);
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with resources created by a factory.
    /// </summary>
    /// <param name="descriptionResourcesFactory">
    /// The factory that creates description resource managers, in priority order.
    /// </param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="descriptionResourcesFactory"/> is <see langword="null"/>.
    /// </exception>
    public HelpConfigurationBuilder UseLocalizedTextProvider(
        Func<IServiceProvider?, IEnumerable<ResourceManager>> descriptionResourcesFactory)
    {
        ArgumentNullException.ThrowIfNull(descriptionResourcesFactory);
        _builder.UseLocalizedHelpTextProviderCore(descriptionResourcesFactory);
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with the specified description resources.
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
    public HelpConfigurationBuilder UseLocalizedTextProvider(
        string baseName,
        Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseName);
        ArgumentNullException.ThrowIfNull(assembly);

        var resourceManager = new ResourceManager(baseName, assembly);
        return UseLocalizedTextProvider(resourceManager);
    }
}
