// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.SpectreConsole;

/// <summary>
/// Provides ANSI consoles for standard output and standard error.
/// </summary>
public interface IAnsiConsoleWriter : IAnsiConsoleOutputWriter, IAnsiConsoleErrorWriter
{
}
