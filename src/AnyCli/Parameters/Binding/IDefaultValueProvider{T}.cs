// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Binding;

/// <summary>
/// Creates a computed default value for a command parameter.
/// </summary>
/// <typeparam name="T">The parameter property type.</typeparam>
public interface IDefaultValueProvider<out T>
{
    /// <summary>
    /// Creates the default value used when the parameter is not supplied.
    /// </summary>
    /// <returns>The default value.</returns>
    T CreateDefault();
}
