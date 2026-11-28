// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
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
    /// Uses the specified help text provider.
    /// </summary>
    /// <param name="textProvider">The help text provider.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textProvider"/> is <see langword="null"/>.
    /// </exception>
    public HelpConfigurationBuilder UseTextProvider(IHelpTextProvider textProvider)
    {
        _builder.HelpFormatting.UseTextProvider(textProvider);
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with the specified description resources.
    /// </summary>
    /// <param name="descriptionResources">The resource managers used to resolve descriptions.</param>
    /// <returns>The current builder.</returns>
    public HelpConfigurationBuilder UseLocalizedTextResources(
        params ResourceManager[] descriptionResources)
    {
        _builder.HelpFormatting.UseLocalizedTextResources(descriptionResources);
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
    /// Registers the renderer used for help output.
    /// </summary>
    /// <typeparam name="TRenderer">The help renderer type.</typeparam>
    /// <returns>The current builder.</returns>
    /// <remarks>
    /// When the renderer is registered in the configured service provider, that instance is used;
    /// otherwise, a public parameterless constructor is used.
    /// </remarks>
    public HelpConfigurationBuilder UseRenderer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TRenderer>()
        where TRenderer : class, IHelpRenderer
    {
        _builder.UseHelpRendererCore<TRenderer>();
        return this;
    }

    /// <summary>
    /// Registers the factory used to create the help renderer.
    /// </summary>
    /// <param name="factory">The factory that creates the help renderer.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="factory"/> is <see langword="null"/>.
    /// </exception>
    public HelpConfigurationBuilder UseRenderer(
        Func<HelpRendererContext, IHelpRenderer> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _builder.UseHelpRendererCore(factory);
        return this;
    }
}
