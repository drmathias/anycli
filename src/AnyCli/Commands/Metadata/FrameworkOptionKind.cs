// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Metadata;

/// <summary>
/// Identifies the behavior of a framework-provided option.
/// </summary>
public enum FrameworkOptionKind
{
    /// <summary>
    /// The option requests help output.
    /// </summary>
    Help,

    /// <summary>
    /// The option requests the application version.
    /// </summary>
    Version,
}
