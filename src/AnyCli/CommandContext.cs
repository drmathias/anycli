// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the runtime context for a command execution.
/// </summary>
public sealed class CommandContext
{
    internal CommandContext(
        IReadOnlyList<string> commandPath,
        IConsole console,
        CommandOptionValues globalOptions)
    {
        CommandPath = commandPath;
        Name = commandPath[^1];
        Console = console;
        GlobalOptionValues = globalOptions;
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
    /// Gets the parsed global option values that apply to every command.
    /// </summary>
    public CommandOptionValues GlobalOptionValues { get; }
}
