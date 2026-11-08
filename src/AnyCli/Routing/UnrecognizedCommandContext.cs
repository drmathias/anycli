// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Routing;

/// <summary>
/// Provides the data needed to handle an unrecognized command.
/// </summary>
/// <param name="AppName">The application name, or <see langword="null"/> when not configured.</param>
/// <param name="AttemptedCommandPath">The attempted command path.</param>
/// <param name="AvailableCommandPaths">The available command paths at the current level.</param>
/// <param name="Console">The console used by the application.</param>
public sealed record UnrecognizedCommandContext(
    string? AppName,
    string AttemptedCommandPath,
    IReadOnlyList<string> AvailableCommandPaths,
    IConsole Console)
{
    /// <summary>
    /// Gets a value indicating whether the command was recognized but requires a subcommand.
    /// </summary>
    public bool IsMissingSubcommand { get; init; }
}
