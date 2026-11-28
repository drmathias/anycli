// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides the inputs used to create a help renderer.
/// </summary>
/// <param name="ServiceProvider">The application service provider, when one is configured.</param>
/// <param name="TextProvider">The configured help text provider.</param>
/// <param name="ValueFormatter">The configured help value formatter.</param>
/// <param name="Localization">The configured localization options.</param>
public sealed record HelpRendererContext(
    IServiceProvider? ServiceProvider,
    IHelpTextProvider TextProvider,
    IHelpValueFormatter ValueFormatter,
    LocalizationOptions Localization);
