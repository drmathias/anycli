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
        IOutputWriter output,
        string? commandName,
        IReadOnlyList<CommandInfo> commands,
        IReadOnlyList<OptionInfo> options)
    {
        AppName = appName;
        Output = output;
        CommandName = commandName;
        Commands = commands;
        Options = options;
    }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public string AppName { get; }

    /// <summary>
    /// Gets the writer used for help output.
    /// </summary>
    public IOutputWriter Output { get; }

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
