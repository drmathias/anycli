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
    /// Uses the default Spectre.Console help renderer.
    /// </summary>
    /// <param name="help">The help configuration builder.</param>
    /// <param name="configure">An optional callback for configuring the renderer.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="help"/> is <see langword="null"/>.
    /// </exception>
    public static HelpConfigurationBuilder UseSpectreHelpRenderer(
        this HelpConfigurationBuilder help,
        Action<SpectreHelpOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(help);

        var options = new SpectreHelpOptions();
        configure?.Invoke(options);

        return help.UseRenderer(context => new SpectreHelpRenderer(
            context.TextProvider,
            context.ValueFormatter,
            options));
    }
}
