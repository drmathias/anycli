// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Marks a parameters property as a required positional command argument.
/// </summary>
public class ArgumentAttribute : ArgumentAttributeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentAttribute"/> class.
    /// </summary>
    /// <param name="position">The zero-based position of the argument.</param>
    public ArgumentAttribute(int position)
        : base(position)
    {
    }
}
