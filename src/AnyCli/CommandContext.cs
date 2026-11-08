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
        string name,
        IConsole console,
        CommandOptionValues globalOptions)
    {
        Name = name;
        Console = console;
        GlobalOptions = globalOptions;
    }

    /// <summary>
    /// Gets the name the command was registered under.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the console used by the command.
    /// </summary>
    public IConsole Console { get; }

    /// <summary>
    /// Gets the parsed options that apply to every command.
    /// </summary>
    public CommandOptionValues GlobalOptions { get; }
}
