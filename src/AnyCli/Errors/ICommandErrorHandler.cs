// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Errors;

/// <summary>
/// Handles command-line errors.
/// </summary>
public interface ICommandErrorHandler
{
    /// <summary>
    /// Handles a command-line error and returns the process exit code.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The process exit code.</returns>
    ValueTask<int> HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken);
}

/// <summary>
/// Handles command-line errors using a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public interface ICommandErrorHandler<TExitCode> : ICommandErrorHandler
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Handles a command-line error.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The exit code.</returns>
    new ValueTask<TExitCode> HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken);

    async ValueTask<int> ICommandErrorHandler.HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken)
    {
        var result = await HandleAsync(context, cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }
}
