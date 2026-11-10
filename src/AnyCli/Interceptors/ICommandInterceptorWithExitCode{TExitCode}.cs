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
    /// <returns>The command exit code.</returns>
    Task<TExitCode> InterceptAsync(CommandContext context, Func<Task<TExitCode>> next);

    async Task<int> ICommandInterceptor.InterceptAsync(
        CommandContext context,
        Func<Task<int>> next)
    {
        var result = await InterceptAsync(
            context,
            async () => (TExitCode)Enum.ToObject(
                typeof(TExitCode),
                await next().ConfigureAwait(false))).ConfigureAwait(false);

        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }
}
