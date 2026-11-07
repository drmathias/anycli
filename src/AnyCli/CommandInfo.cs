// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a registered command.
/// </summary>
/// <param name="Name">The command name.</param>
/// <param name="Description">A short description of the command.</param>
public sealed record CommandInfo(string Name, string Description)
{
    /// <summary>
    /// Gets the command's positional arguments.
    /// </summary>
    public IReadOnlyList<ArgumentInfo> Arguments { get; init; } = [];
}
