// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Commands.Registration;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Provides Spectre.Console-specific error configuration.
/// </summary>
public static class CommandErrorHandlingBuilderExtensions
{
    /// <summary>
    /// Uses the Spectre.Console command-line error writer.
    /// </summary>
    /// <param name="errors">The error handling builder.</param>
    /// <param name="options">The styling options, or <see langword="null"/> to use defaults.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="errors"/> is <see langword="null"/>.
    /// </exception>
    public static CommandErrorHandlingBuilder UseSpectreErrorWriter(
        this CommandErrorHandlingBuilder errors,
        SpectreErrorOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return errors.UseWriter(new SpectreCommandErrorWriter(options));
    }
}
