// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Routing;

/// <summary>
/// Handles an unrecognized command.
/// </summary>
public interface IUnrecognizedCommandHandler
{
    /// <summary>
    /// Writes the error for an unrecognized command.
    /// </summary>
    /// <param name="context">The unrecognized command context.</param>
    void Handle(UnrecognizedCommandContext context);
}
