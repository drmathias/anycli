// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Writes help output for the application or a single command.
/// </summary>
public interface IHelpProvider
{
    /// <summary>
    /// Writes root help output.
    /// </summary>
    /// <param name="context">The root help context.</param>
    void WriteRootHelp(RootHelpContext context);

    /// <summary>
    /// Writes help output for a command.
    /// </summary>
    /// <param name="context">The command help context.</param>
    void WriteCommandHelp(CommandHelpContext context);
}
