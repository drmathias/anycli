// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Console;

/// <summary>
/// Provides the standard output and error writers for a command.
/// </summary>
public interface IConsoleWriter : IConsoleOutputWriter, IConsoleErrorWriter;
