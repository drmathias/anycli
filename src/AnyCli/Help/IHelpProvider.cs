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
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the help output has been written.</returns>
    ValueTask WriteRootHelpAsync(
        RootHelpContext context,
        CancellationToken cancellationToken);

    /// <summary>
    /// Writes help output for a command.
    /// </summary>
    /// <param name="context">The command help context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the help output has been written.</returns>
    ValueTask WriteCommandHelpAsync(
        CommandHelpContext context,
        CancellationToken cancellationToken);
}
