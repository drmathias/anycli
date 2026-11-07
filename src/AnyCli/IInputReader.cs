// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// A read-only text input used for standard input.
/// </summary>
public interface IInputReader
{
    /// <summary>
    /// Reads a line of text.
    /// </summary>
    /// <returns>
    /// The next line from the input, or <see langword="null"/> when no more input is available.
    /// </returns>
    string? ReadLine();
}
