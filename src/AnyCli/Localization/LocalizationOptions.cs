// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Localization;

/// <summary>
/// Configures the cultures used for localized command-line text.
/// </summary>
public sealed record LocalizationOptions
{
    /// <summary>
    /// Gets the provider used to choose the UI culture.
    /// </summary>
    public Func<CultureInfo> UICultureProvider { get; init; } =
        static () => CultureInfo.CurrentUICulture;

    /// <summary>
    /// Gets the provider used to choose the formatting culture.
    /// </summary>
    public Func<CultureInfo> FormatCultureProvider { get; init; } =
        static () => CultureInfo.CurrentCulture;

    /// <summary>
    /// Gets the default localization options.
    /// </summary>
    public static LocalizationOptions Default { get; } = new();

    /// <summary>
    /// Creates options that use the same culture for UI and formatting.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <returns>The localization options.</returns>
    /// <exception cref="CultureNotFoundException">
    /// <paramref name="cultureName"/> is not a valid culture name.
    /// </exception>
    public static LocalizationOptions FromCulture(string cultureName) =>
        FromCulture(CultureInfo.GetCultureInfo(cultureName));

    /// <summary>
    /// Creates options that use the same culture for UI and formatting.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <returns>The localization options.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="culture"/> is <see langword="null"/>.
    /// </exception>
    public static LocalizationOptions FromCulture(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);
        return FromCulture(() => culture);
    }

    /// <summary>
    /// Creates options that use the same culture provider for UI and formatting.
    /// </summary>
    /// <param name="cultureProvider">The culture provider.</param>
    /// <returns>The localization options.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cultureProvider"/> is <see langword="null"/>.
    /// </exception>
    public static LocalizationOptions FromCulture(Func<CultureInfo> cultureProvider)
    {
        ArgumentNullException.ThrowIfNull(cultureProvider);
        return new LocalizationOptions
        {
            UICultureProvider = cultureProvider,
            FormatCultureProvider = cultureProvider,
        };
    }

    /// <summary>
    /// Gets the UI culture.
    /// </summary>
    /// <returns>The UI culture.</returns>
    /// <exception cref="InvalidOperationException">
    /// The UI culture provider returned <see langword="null"/>.
    /// </exception>
    public CultureInfo GetUICulture() =>
        UICultureProvider() ?? throw new InvalidOperationException(
            "The localization UI culture provider returned null.");

    /// <summary>
    /// Gets the formatting culture.
    /// </summary>
    /// <returns>The formatting culture.</returns>
    /// <exception cref="InvalidOperationException">
    /// The formatting culture provider returned <see langword="null"/>.
    /// </exception>
    public CultureInfo GetFormatCulture() =>
        FormatCultureProvider() ?? throw new InvalidOperationException(
            "The localization formatting culture provider returned null.");
}
