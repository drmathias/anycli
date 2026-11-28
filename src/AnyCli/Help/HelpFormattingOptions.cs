// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Resources;


namespace AnyCli.Help;

/// <summary>
/// Configures formatting services used by help renderers.
/// </summary>
internal sealed class HelpFormattingOptions
{
    private Func<HelpRendererContext, IHelpTextProvider>? _textProviderFactory;
    private Func<HelpRendererContext, IHelpValueFormatter>? _valueFormatterFactory;

    /// <summary>
    /// Uses the specified help text provider type.
    /// </summary>
    /// <typeparam name="TProvider">The help text provider type.</typeparam>
    /// <returns>The current options.</returns>
    public HelpFormattingOptions UseTextProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>()
        where TProvider : class, IHelpTextProvider
    {
        _textProviderFactory = context => ResolveTextProvider<TProvider>(context.ServiceProvider);
        return this;
    }

    /// <summary>
    /// Uses the specified help text provider.
    /// </summary>
    /// <param name="textProvider">The help text provider.</param>
    /// <returns>The current options.</returns>
    public HelpFormattingOptions UseTextProvider(IHelpTextProvider textProvider)
    {
        ArgumentNullException.ThrowIfNull(textProvider);
        _textProviderFactory = _ => textProvider;
        return this;
    }

    /// <summary>
    /// Uses the localized help text provider with the specified description resources.
    /// </summary>
    /// <param name="descriptionResources">The resource managers used to resolve descriptions.</param>
    /// <returns>The current options.</returns>
    public HelpFormattingOptions UseLocalizedTextResources(
        params ResourceManager[] descriptionResources)
    {
        ArgumentNullException.ThrowIfNull(descriptionResources);
        _textProviderFactory = context => new LocalizedHelpTextProvider(
            descriptionResources,
            context.Localization);
        return this;
    }

    /// <summary>
    /// Uses the specified default value formatter type.
    /// </summary>
    /// <typeparam name="TFormatter">The formatter type.</typeparam>
    /// <returns>The current options.</returns>
    public HelpFormattingOptions UseValueFormatter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFormatter>()
        where TFormatter : class, IHelpValueFormatter
    {
        _valueFormatterFactory = context => ResolveValueFormatter<TFormatter>(context.ServiceProvider);
        return this;
    }

    /// <summary>
    /// Uses the specified default value formatter.
    /// </summary>
    /// <param name="formatter">The formatter.</param>
    /// <returns>The current options.</returns>
    public HelpFormattingOptions UseValueFormatter(IHelpValueFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        _valueFormatterFactory = _ => formatter;
        return this;
    }

    /// <summary>
    /// Creates the configured help text provider.
    /// </summary>
    /// <param name="context">The help renderer context.</param>
    /// <returns>The help text provider.</returns>
    internal IHelpTextProvider CreateTextProvider(HelpRendererContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return _textProviderFactory?.Invoke(context) ?? context.TextProvider;
    }

    /// <summary>
    /// Creates the configured default value formatter.
    /// </summary>
    /// <param name="context">The help renderer context.</param>
    /// <returns>The default value formatter.</returns>
    internal IHelpValueFormatter CreateValueFormatter(HelpRendererContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return _valueFormatterFactory?.Invoke(context) ?? context.ValueFormatter;
    }

    private static TProvider ResolveTextProvider<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>(
        IServiceProvider? serviceProvider)
        where TProvider : class, IHelpTextProvider =>
        ServiceResolver.Resolve<TProvider>(serviceProvider, "help text provider");

    private static TFormatter ResolveValueFormatter<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TFormatter>(
        IServiceProvider? serviceProvider)
        where TFormatter : class, IHelpValueFormatter =>
        ServiceResolver.Resolve<TFormatter>(serviceProvider, "help value formatter");
}
