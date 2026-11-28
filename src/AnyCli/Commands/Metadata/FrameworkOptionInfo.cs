// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Metadata;

/// <summary>
/// Describes an option provided by the framework rather than declared by the application.
/// </summary>
/// <param name="Kind">The framework behavior the option triggers.</param>
/// <param name="Names">The option names, such as <c>-h</c> and <c>--help</c>.</param>
/// <param name="Description">A short description of the option.</param>
public sealed record FrameworkOptionInfo(
    FrameworkOptionKind Kind,
    IReadOnlyList<string> Names,
    HelpText Description);
