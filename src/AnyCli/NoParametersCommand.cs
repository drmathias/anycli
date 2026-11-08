// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Base class for synchronous commands that do not use parameters.
/// </summary>
public abstract class NoParametersCommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>The process exit code.</returns>
    public abstract int Execute(CommandContext context);
}

/// <summary>
/// Base class for synchronous commands that do not use parameters and return a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
public abstract class NoParametersCommand<TExitCode>
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>The command's exit code.</returns>
    public abstract TExitCode Execute(CommandContext context);
}
