// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Provides the data needed to handle a command-line error.
/// </summary>
/// <param name="AppName">The application name, or <see langword="null"/> when not configured.</param>
/// <param name="CommandPath">The command path associated with the error.</param>
/// <param name="Error">The structured command error.</param>
/// <param name="Console">The console writer for standard error.</param>
public sealed record CommandErrorContext(
    string? AppName,
    IReadOnlyList<string> CommandPath,
    CommandError Error,
    IConsoleErrorWriter Console);
