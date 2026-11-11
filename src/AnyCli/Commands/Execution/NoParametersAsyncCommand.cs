// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Execution;

/// <summary>
/// Base class for asynchronous commands that do not use parameters.
/// </summary>
public abstract class NoParametersAsyncCommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>A task that produces the process exit code.</returns>
    public abstract Task<int> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken);
}

/// <summary>
/// Base class for asynchronous commands that do not use parameters and return a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
public abstract class NoParametersAsyncCommand<TExitCode>
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>A task that produces the command's exit code.</returns>
    public abstract Task<TExitCode> ExecuteAsync(
        CommandContext context,
        CancellationToken cancellationToken);
}
