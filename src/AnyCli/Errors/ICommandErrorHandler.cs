// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Handles command-line errors.
/// </summary>
public interface ICommandErrorHandler
{
    /// <summary>
    /// Handles a command-line error.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the error has been handled.</returns>
    ValueTask HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken);
}
