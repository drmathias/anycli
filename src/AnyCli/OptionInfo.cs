// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a command-line option.
/// </summary>
/// <param name="Names">The option names, such as <c>-h</c> and <c>--help</c>.</param>
/// <param name="Description">A short description of the option.</param>
/// <param name="IsRequired">Whether the option must be supplied.</param>
/// <param name="TakesValue">Whether the option consumes a value.</param>
/// <param name="ValueType">The option value or collection type.</param>
public sealed record OptionInfo(
    IReadOnlyList<string> Names,
    string Description,
    bool IsRequired = false,
    bool TakesValue = false,
    Type? ValueType = null);
