// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Interceptors;

/// <summary>
/// Intercepts command execution for commands that return a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public interface ICommandInterceptorWithExitCode<TExitCode> : ICommandInterceptor
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Intercepts command execution.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="next">The next interceptor or the command execution delegate.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>The command exit code.</returns>
    Task<TExitCode> InterceptAsync(
        CommandContext context,
        Func<CancellationToken, Task<TExitCode>> next,
        CancellationToken cancellationToken);

    async Task<int> ICommandInterceptor.InterceptAsync(
        CommandContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken)
    {
        var result = await InterceptAsync(
            context,
            async token => (TExitCode)Enum.ToObject(
                typeof(TExitCode),
                await next(token).ConfigureAwait(false)),
            cancellationToken).ConfigureAwait(false);

        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }
}
