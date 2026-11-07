// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Writes help output for the application or a single command.
/// </summary>
public interface IHelpProvider
{
    /// <summary>
    /// Writes help text to <see cref="HelpContext.Console"/>.
    /// </summary>
    /// <param name="context">The help context.</param>
    void WriteHelp(HelpContext context);
}
