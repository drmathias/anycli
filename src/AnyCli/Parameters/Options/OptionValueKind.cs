// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

/// <summary>
/// Describes how an option consumes command-line values.
/// </summary>
public enum OptionValueKind
{
    /// <summary>
    /// The option is a flag and does not consume a value.
    /// </summary>
    Flag,

    /// <summary>
    /// The option consumes a single value.
    /// </summary>
    Value,

    /// <summary>
    /// The option consumes repeated values.
    /// </summary>
    Collection,
}
