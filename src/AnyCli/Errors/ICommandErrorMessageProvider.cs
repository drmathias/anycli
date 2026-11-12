// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Provides user-facing messages for command-line errors.
/// </summary>
public interface ICommandErrorMessageProvider
{
    /// <summary>
    /// Gets the message for a command-line error.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <returns>The plain-text error message.</returns>
    string GetMessage(CommandErrorContext context);
}
