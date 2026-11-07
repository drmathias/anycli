// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the standard output and error writers for a command.
/// </summary>
public interface IConsoleWriter
{
    /// <summary>
    /// Gets the writer used for standard output.
    /// </summary>
    IOutputWriter Output { get; }

    /// <summary>
    /// Gets the writer used for standard error.
    /// </summary>
    IOutputWriter Error { get; }
}
