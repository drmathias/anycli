// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a command-line option.
/// </summary>
/// <param name="Names">The option names, such as <c>-h</c> and <c>--help</c>.</param>
/// <param name="Description">A short description of the option.</param>
public sealed record OptionInfo(IReadOnlyList<string> Names, string Description);
