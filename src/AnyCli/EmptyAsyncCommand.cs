// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Base class for asynchronous commands that do not use settings.
/// </summary>
public abstract class EmptyAsyncCommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>A task that produces the process exit code.</returns>
    public abstract Task<int> ExecuteAsync(CommandContext context);
}

/// <summary>
/// Base class for asynchronous commands that do not use settings and return a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
public abstract class EmptyAsyncCommand<TExitCode>
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>A task that produces the command's exit code.</returns>
    public abstract Task<TExitCode> ExecuteAsync(CommandContext context);
}
