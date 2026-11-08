// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides the data needed to render help output for a command or command group.
/// </summary>
public sealed class CommandHelpContext : HelpContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandHelpContext"/> class.
    /// </summary>
    /// <param name="appName">The application name.</param>
    /// <param name="console">The console writer.</param>
    /// <param name="globalOptions">The application-wide options.</param>
    /// <param name="command">The command or command group being described.</param>
    public CommandHelpContext(
        string appName,
        IConsoleWriter console,
        IReadOnlyList<OptionInfo> globalOptions,
        CommandInfo command)
        : base(appName, console, globalOptions)
    {
        Command = command;
    }

    /// <summary>
    /// Gets the command or command group being described.
    /// </summary>
    public CommandInfo Command { get; }
}
