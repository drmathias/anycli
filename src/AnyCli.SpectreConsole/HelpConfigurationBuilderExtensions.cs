// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Commands.Registration;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Provides Spectre.Console-specific help configuration.
/// </summary>
public static class HelpConfigurationBuilderExtensions
{
    /// <summary>
    /// Uses the default Spectre.Console help provider.
    /// </summary>
    /// <param name="help">The help configuration builder.</param>
    /// <param name="options">The styling options, or <see langword="null"/> to use defaults.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="help"/> is <see langword="null"/>.
    /// </exception>
    public static HelpConfigurationBuilder UseSpectreHelp(
        this HelpConfigurationBuilder help,
        SpectreHelpOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(help);
        return help.UseProvider(context =>
            new SpectreHelpProvider(context.TextProvider, options));
    }
}
