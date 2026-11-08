// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Console;

/// <summary>
/// A write-only text output used for standard output or standard error.
/// </summary>
/// <remarks>
/// This intentionally exposes only writing operations. Disposal and flushing
/// are handled by the application host.
/// </remarks>
public interface IOutputWriter
{
    /// <summary>
    /// Writes text without a trailing line break.
    /// </summary>
    /// <param name="value">The text to write.</param>
    void Write(string? value);

    /// <summary>
    /// Writes a line break.
    /// </summary>
    void WriteLine();

    /// <summary>
    /// Writes text followed by a line break.
    /// </summary>
    /// <param name="value">The text to write.</param>
    void WriteLine(string? value);
}
