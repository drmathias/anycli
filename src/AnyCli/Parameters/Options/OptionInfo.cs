// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

/// <summary>
/// Describes a command-line option.
/// </summary>
/// <param name="Names">The option names, such as <c>-h</c> and <c>--help</c>.</param>
/// <param name="Description">A short description of the option.</param>
/// <param name="IsRequired">Whether the option must be supplied.</param>
/// <param name="ValueKind">How the option consumes command-line values.</param>
/// <param name="DeclaredType">The declared option value or collection type.</param>
public sealed record OptionInfo(
    IReadOnlyList<string> Names,
    HelpText Description,
    bool IsRequired,
    OptionValueKind ValueKind,
    Type? DeclaredType = null)
{
    /// <summary>
    /// Gets the configured default value source, when one is defined.
    /// </summary>
    public DefaultValueInfo? DefaultValue { get; init; }
}
