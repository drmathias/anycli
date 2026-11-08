// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Execution;

/// <summary>
/// A command that executes asynchronously with typed parameters.
/// </summary>
/// <typeparam name="TParameters">The parameters type.</typeparam>
public interface IAsyncCommand<TParameters>
    where TParameters : class
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="parameters">The parsed command parameters.</param>
    /// <returns>A task that produces the process exit code.</returns>
    Task<int> ExecuteAsync(CommandContext context, TParameters parameters);
}

/// <summary>
/// A command that executes asynchronously with typed parameters and returns a custom exit code enum.
/// </summary>
/// <typeparam name="TParameters">The parameters type.</typeparam>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
public interface IAsyncCommand<TParameters, TExitCode>
    where TParameters : class
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="parameters">The parsed command parameters.</param>
    /// <returns>A task that produces the command's exit code.</returns>
    Task<TExitCode> ExecuteAsync(CommandContext context, TParameters parameters);
}
