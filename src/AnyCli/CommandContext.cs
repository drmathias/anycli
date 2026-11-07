// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the runtime context for a command execution.
/// </summary>
public sealed class CommandContext
{
    internal CommandContext(string name, IConsole console)
    {
        Name = name;
        Console = console;
    }

    /// <summary>
    /// Gets the name the command was registered under.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the console used by the command.
    /// </summary>
    public IConsole Console { get; }
}
