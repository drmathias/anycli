// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Provides the configured services used to create a command-line error handler.
/// </summary>
/// <param name="ServiceProvider">The application service provider, when one is configured.</param>
/// <param name="MessageProvider">The configured command-line error message provider.</param>
/// <param name="Writer">The configured command-line error writer.</param>
/// <param name="Localization">The configured localization options.</param>
public sealed record CommandErrorHandlerContext(
    IServiceProvider? ServiceProvider,
    ICommandErrorMessageProvider MessageProvider,
    ICommandErrorWriter Writer,
    LocalizationOptions Localization)
{
    /// <summary>
    /// Gets a handler that uses the configured message provider and writer.
    /// </summary>
    public ICommandErrorHandler DefaultHandler =>
        new DefaultCommandErrorHandler(MessageProvider, Writer);
}
