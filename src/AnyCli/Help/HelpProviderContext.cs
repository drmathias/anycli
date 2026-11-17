// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides the inputs used to create a help provider.
/// </summary>
/// <param name="ServiceProvider">The application service provider, when one is configured.</param>
/// <param name="TextProvider">The configured help text provider.</param>
public sealed record HelpProviderContext(
    IServiceProvider? ServiceProvider,
    IHelpTextProvider TextProvider);
