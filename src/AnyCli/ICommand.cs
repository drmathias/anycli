// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// A command that executes synchronously and returns a process exit code.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>The process exit code.</returns>
    int Execute(CommandContext context);
}
