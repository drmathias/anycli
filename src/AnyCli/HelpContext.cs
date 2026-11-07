// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the data needed to render help output.
/// </summary>
public sealed class HelpContext
{
    internal HelpContext(
        string appName,
        IConsoleWriter console,
        string? commandName,
        IReadOnlyList<CommandInfo> commands,
        IReadOnlyList<OptionInfo> options)
    {
        AppName = appName;
        Console = console;
        CommandName = commandName;
        Commands = commands;
        Options = options;
    }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public string AppName { get; }

    /// <summary>
    /// Gets the console writer used for help output.
    /// </summary>
    public IConsoleWriter Console { get; }

    /// <summary>
    /// Gets the command name being described, or <see langword="null"/> for root help.
    /// </summary>
    public string? CommandName { get; }

    /// <summary>
    /// Gets the registered commands.
    /// </summary>
    public IReadOnlyList<CommandInfo> Commands { get; }

    /// <summary>
    /// Gets the available application options.
    /// </summary>
    public IReadOnlyList<OptionInfo> Options { get; }
}
