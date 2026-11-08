// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// A command that executes synchronously with typed parameters.
/// </summary>
/// <typeparam name="TParameters">The parameters type.</typeparam>
public interface ICommand<TParameters>
    where TParameters : class
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="parameters">The parsed command parameters.</param>
    /// <returns>The process exit code.</returns>
    int Execute(CommandContext context, TParameters parameters);
}

/// <summary>
/// A command that executes synchronously with typed parameters and returns a custom exit code enum.
/// </summary>
/// <typeparam name="TParameters">The parameters type.</typeparam>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
public interface ICommand<TParameters, TExitCode>
    where TParameters : class
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="parameters">The parsed command parameters.</param>
    /// <returns>The command's exit code.</returns>
    TExitCode Execute(CommandContext context, TParameters parameters);
}
