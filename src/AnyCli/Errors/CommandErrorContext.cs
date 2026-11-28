// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Provides the data needed to handle a command-line error.
/// </summary>
/// <param name="Application">The application metadata.</param>
/// <param name="CommandPath">The command path associated with the error.</param>
/// <param name="Error">The structured command error.</param>
/// <param name="Console">The console writer for standard error.</param>
public sealed record CommandErrorContext(
    ApplicationInfo Application,
    IReadOnlyList<string> CommandPath,
    CommandError Error,
    IConsoleErrorWriter Console);
