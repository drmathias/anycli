// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

/// <summary>
/// Marks a partial class as a source-generated global options type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GlobalOptionsAttribute : Attribute
{
}
