// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Console;

/// <summary>
/// Provides the standard output writer for a command.
/// </summary>
public interface IConsoleOutputWriter
{
    /// <summary>
    /// Gets the writer used for standard output.
    /// </summary>
    IOutputWriter Output { get; }
}
