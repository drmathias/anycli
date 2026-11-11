// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Routing;

/// <summary>
/// Handles an unrecognized command.
/// </summary>
public interface IUnrecognizedCommandHandler
{
    /// <summary>
    /// Writes the error for an unrecognized command.
    /// </summary>
    /// <param name="context">The unrecognized command context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the error has been written.</returns>
    ValueTask HandleAsync(
        UnrecognizedCommandContext context,
        CancellationToken cancellationToken);
}
