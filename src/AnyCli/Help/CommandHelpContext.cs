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
    /// <param name="console">The console writer.</param>
    /// <param name="application">The application metadata.</param>
    /// <param name="command">The command or command group being described.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="command"/> is <see langword="null"/>.
    /// </exception>
    public CommandHelpContext(
        IConsoleWriter console,
        ApplicationInfo application,
        CommandInfo command)
        : base(console, application)
    {
        ArgumentNullException.ThrowIfNull(command);
        Command = command;
    }

    /// <summary>
    /// Gets the command or command group being described.
    /// </summary>
    public CommandInfo Command { get; }
}
