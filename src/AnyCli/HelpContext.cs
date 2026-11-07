// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

public sealed class HelpContext
{
    internal HelpContext(
        string appName,
        TextWriter output,
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

    public string AppName { get; }

    public TextWriter Output { get; }

    public string? CommandName { get; }

    public IReadOnlyList<CommandInfo> Commands { get; }

    public IReadOnlyList<OptionInfo> Options { get; }
}
