// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the runtime context for a command execution.
/// </summary>
public sealed class CommandContext
{
    internal CommandContext(string name, IOutputWriter output, IOutputWriter error)
    {
        Name = name;
        Output = output;
        Error = error;
    }

    /// <summary>
    /// Gets the name the command was registered under.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the writer used for standard output.
    /// </summary>
    public IOutputWriter Output { get; }

    /// <summary>
    /// Gets the writer used for error output.
    /// </summary>
    public IOutputWriter Error { get; }
}
