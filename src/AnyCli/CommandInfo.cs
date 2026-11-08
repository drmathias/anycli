// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a registered command or command group.
/// </summary>
/// <param name="Name">The command name.</param>
/// <param name="Description">A short description of the command.</param>
public sealed record CommandInfo(string Name, string Description)
{
    /// <summary>
    /// Gets the command path, starting at the root command.
    /// </summary>
    public IReadOnlyList<string> Path { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether this node has an executable command.
    /// </summary>
    public bool IsExecutable { get; init; }

    /// <summary>
    /// Gets a value indicating whether the command or group has subcommands.
    /// </summary>
    public bool HasSubcommands { get; init; }

    /// <summary>
    /// Gets the command's positional arguments.
    /// </summary>
    public IReadOnlyList<ArgumentInfo> Arguments { get; init; } = [];

    /// <summary>
    /// Gets the command's named options.
    /// </summary>
    public IReadOnlyList<OptionInfo> Options { get; init; } = [];

    /// <summary>
    /// Gets the child commands and groups.
    /// </summary>
    public IReadOnlyList<CommandInfo> Children { get; init; } = [];
}
