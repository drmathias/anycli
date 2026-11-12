// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// The exception thrown when command-line input cannot be processed.
/// </summary>
public sealed class CommandErrorException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandErrorException"/> class.
    /// </summary>
    /// <param name="error">The structured command error.</param>
    /// <param name="commandPath">The command path associated with the error.</param>
    public CommandErrorException(
        CommandError error,
        IReadOnlyList<string>? commandPath = null)
        : base(error.ToString())
    {
        ArgumentNullException.ThrowIfNull(error);

        Error = error;
        CommandPath = commandPath ?? [];
    }

    /// <summary>
    /// Gets the structured command error.
    /// </summary>
    public CommandError Error { get; }

    /// <summary>
    /// Gets the command path associated with the error.
    /// </summary>
    public IReadOnlyList<string> CommandPath { get; }
}
