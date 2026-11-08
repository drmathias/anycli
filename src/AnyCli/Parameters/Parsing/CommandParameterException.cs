// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// The exception thrown when command-line parameters cannot be bound to a command.
/// </summary>
public sealed class CommandParameterException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandParameterException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CommandParameterException(string message)
        : base(message)
    {
    }
}
