// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides the data needed to resolve a help description.
/// </summary>
/// <param name="Command">The command that owns the description, or <see langword="null"/> for root options.</param>
/// <param name="Description">The configured description.</param>
/// <param name="Option">The option being described, when applicable.</param>
/// <param name="Argument">The argument being described, when applicable.</param>
public sealed record HelpDescriptionContext(
    CommandInfo? Command,
    HelpText Description,
    OptionInfo? Option = null,
    ArgumentInfo? Argument = null);
