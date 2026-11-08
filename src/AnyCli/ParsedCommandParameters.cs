// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the parsed positional arguments and named options for a command.
/// </summary>
public sealed class ParsedCommandParameters
{
    internal ParsedCommandParameters(
        CommandArgumentValues arguments,
        CommandOptionValues options)
    {
        Arguments = arguments;
        Options = options;
    }

    /// <summary>
    /// Gets the parsed positional argument values.
    /// </summary>
    public CommandArgumentValues Arguments { get; }

    /// <summary>
    /// Gets the parsed named option values.
    /// </summary>
    public CommandOptionValues Options { get; }
}
