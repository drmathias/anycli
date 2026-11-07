// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// The exception thrown when command-line arguments cannot be bound to a command.
/// </summary>
public sealed class CommandArgumentException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgumentException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public CommandArgumentException(string message)
        : base(message)
    {
    }
}
