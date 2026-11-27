// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Execution;

/// <summary>
/// Provides the runtime context for a command execution.
/// </summary>
public class CommandContext
{
    internal CommandContext(
        IReadOnlyList<string> commandPath,
        IConsole console,
        ApplicationInfo application)
    {
        CommandPath = commandPath;
        Name = commandPath[^1];
        Console = console;
        Application = application;
    }

    /// <summary>
    /// Gets the name the command was registered under.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the full command path, starting at the root command.
    /// </summary>
    public IReadOnlyList<string> CommandPath { get; }

    /// <summary>
    /// Gets the console used by the command.
    /// </summary>
    public IConsole Console { get; }

    /// <summary>
    /// Gets the application metadata.
    /// </summary>
    public ApplicationInfo Application { get; }
}
