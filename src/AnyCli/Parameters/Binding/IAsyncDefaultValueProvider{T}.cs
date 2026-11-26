// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Binding;

/// <summary>
/// Asynchronously creates a computed default value for a command parameter.
/// </summary>
/// <typeparam name="T">The parameter property type.</typeparam>
public interface IAsyncDefaultValueProvider<T>
{
    /// <summary>
    /// Creates the default value used when the parameter is not supplied.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the current command invocation.</param>
    /// <returns>The default value.</returns>
    ValueTask<T> CreateDefaultAsync(CancellationToken cancellationToken);
}
