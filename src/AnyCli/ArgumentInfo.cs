// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a positional command argument.
/// </summary>
/// <param name="Name">The argument name.</param>
/// <param name="Description">A short description of the argument.</param>
/// <param name="IsRequired">Whether the argument is required.</param>
/// <param name="IsVariadic">Whether the argument consumes all remaining values.</param>
public sealed record ArgumentInfo(
    string Name,
    string Description,
    bool IsRequired,
    bool IsVariadic);
