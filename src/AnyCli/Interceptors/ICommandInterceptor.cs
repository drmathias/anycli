// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Interceptors;

/// <summary>
/// Intercepts command execution before and after the selected command runs.
/// </summary>
public interface ICommandInterceptor
{
    /// <summary>
    /// Intercepts command execution.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="next">The next interceptor or the command execution delegate.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>The command exit code.</returns>
    Task<int> InterceptAsync(
        CommandContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken);
}
