// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Resources;

namespace AnyCli.Help;

/// <summary>
/// Provides resource-based, localized text for help output.
/// </summary>
/// <remarks>
/// Derive from this class and override individual methods to customise or localise help text.
/// </remarks>
public class LocalizedHelpTextProvider : IHelpTextProvider
{
    private const string ResourceBaseName = "AnyCli.Help.HelpMessages";

    private static readonly ResourceManager Resources = new(
        ResourceBaseName,
        typeof(LocalizedHelpTextProvider).Assembly);

    private readonly IReadOnlyList<ResourceManager> _descriptionResources;
    private readonly LocalizationOptions _localization;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedHelpTextProvider"/> class.
    /// </summary>
    /// <param name="descriptionResources">
    /// Additional resource managers used to resolve description resource keys, in priority order.
    /// </param>
    /// <param name="localization">
    /// The localization options, or <see langword="null"/> to use the default options.
    /// </param>
    public LocalizedHelpTextProvider(
        IEnumerable<ResourceManager>? descriptionResources = null,
        LocalizationOptions? localization = null)
    {
        _descriptionResources = [.. descriptionResources ?? [], Resources];
        _localization = localization ?? LocalizationOptions.Default;
    }

    /// <inheritdoc/>
    public string GetUsageHeading() => Format(HelpTextResourceKeys.Usage);

    /// <inheritdoc/>
    public string GetDescriptionHeading() => Format(HelpTextResourceKeys.Description);

    /// <inheritdoc/>
    public string GetCommandsHeading() => Format(HelpTextResourceKeys.Commands);

    /// <inheritdoc/>
    public string GetArgumentsHeading() => Format(HelpTextResourceKeys.Arguments);

    /// <inheritdoc/>
    public string GetOptionsHeading() => Format(HelpTextResourceKeys.Options);

    /// <inheritdoc/>
    public virtual string GetDescription(HelpDescriptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!string.IsNullOrWhiteSpace(context.Description.ResourceKey))
        {
            var culture = GetUICulture();
            foreach (var resources in _descriptionResources)
            {
                var resourceValue = TryGetString(
                    resources,
                    context.Description.ResourceKey,
                    culture);

                if (resourceValue is not null)
                {
                    return resourceValue;
                }
            }
        }

        return context.Description.Text ?? string.Empty;
    }

    /// <summary>
    /// Gets the UI culture used to select help text.
    /// </summary>
    /// <returns>The UI culture.</returns>
    protected virtual CultureInfo GetUICulture() =>
        _localization.GetUICulture();

    /// <summary>
    /// Gets the culture used to format values in help text.
    /// </summary>
    /// <returns>The formatting culture.</returns>
    protected virtual CultureInfo GetFormatCulture() =>
        _localization.GetFormatCulture();

    private string Format(string resourceKey) =>
        TryGetString(Resources, resourceKey, GetUICulture()) ?? resourceKey;

    private static string? TryGetString(
        ResourceManager resources,
        string resourceKey,
        CultureInfo culture)
    {
        try
        {
            return resources.GetString(resourceKey, culture);
        }
        catch (MissingManifestResourceException)
        {
            return null;
        }
        catch (MissingSatelliteAssemblyException)
        {
            return null;
        }
    }
}
