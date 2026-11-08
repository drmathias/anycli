// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the data needed to render root help output.
/// </summary>
public sealed class RootHelpContext : HelpContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootHelpContext"/> class.
    /// </summary>
    /// <param name="appName">The application name.</param>
    /// <param name="console">The console writer.</param>
    /// <param name="globalOptions">The application-wide options.</param>
    /// <param name="commands">The registered commands.</param>
    public RootHelpContext(
        string appName,
        IConsoleWriter console,
        IReadOnlyList<OptionInfo> globalOptions,
        IReadOnlyList<CommandInfo> commands)
        : base(appName, console, globalOptions)
    {
        Commands = commands;
    }

    /// <summary>
    /// Gets the registered commands.
    /// </summary>
    public IReadOnlyList<CommandInfo> Commands { get; }
}
