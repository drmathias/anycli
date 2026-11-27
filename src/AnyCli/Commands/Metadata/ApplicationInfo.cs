// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Metadata;

/// <summary>
/// Describes the configured command-line application.
/// </summary>
/// <param name="AppName">The application name.</param>
/// <param name="GlobalOptions">The application-wide options.</param>
/// <param name="Commands">The registered top-level commands and command groups.</param>
public sealed record ApplicationInfo(
    string AppName,
    IReadOnlyList<OptionInfo> GlobalOptions,
    IReadOnlyList<CommandInfo> Commands);
