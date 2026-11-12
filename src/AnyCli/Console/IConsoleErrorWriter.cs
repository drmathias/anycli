// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Console;

/// <summary>
/// Provides the standard error writer for a command.
/// </summary>
public interface IConsoleErrorWriter
{
    /// <summary>
    /// Gets the writer used for standard error.
    /// </summary>
    IOutputWriter Error { get; }
}
