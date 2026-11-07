// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes a positional command argument.
/// </summary>
/// <param name="Name">The argument name.</param>
/// <param name="Description">A short description of the argument.</param>
/// <param name="ArgumentType">The declared argument type.</param>
/// <param name="Arity">The number of values the argument accepts.</param>
public sealed record ArgumentInfo(
    string Name,
    string Description,
    Type ArgumentType,
    ArgumentArity Arity);
