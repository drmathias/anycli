// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// A command that executes asynchronously and returns a process exit code.
/// </summary>
public interface IAsyncCommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>A task that produces the process exit code.</returns>
    Task<int> ExecuteAsync(CommandContext context);
}
