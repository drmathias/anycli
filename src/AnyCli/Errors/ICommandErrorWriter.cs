// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Writes command-line error messages to standard error.
/// </summary>
public interface ICommandErrorWriter
{
    /// <summary>
    /// Writes a command-line error message.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="message">The plain-text error message.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the message has been written.</returns>
    ValueTask WriteAsync(
        CommandErrorContext context,
        string message,
        CancellationToken cancellationToken);
}
