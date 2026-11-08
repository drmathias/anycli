// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Defines the positional arguments and named options accepted by a command.
/// </summary>
public sealed class CommandSettingsBuilder
{
    /// <summary>
    /// Gets the positional argument builder.
    /// </summary>
    public CommandArgumentBuilder Arguments { get; } = new();

    /// <summary>
    /// Gets the named option builder.
    /// </summary>
    public CommandOptionBuilder Options { get; } = new();
}
