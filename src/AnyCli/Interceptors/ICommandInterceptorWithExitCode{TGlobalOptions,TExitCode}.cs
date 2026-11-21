// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli.Interceptors;

/// <summary>
/// Intercepts command execution with typed global options for commands that return a custom
/// exit code enum.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
public interface ICommandInterceptorWithExitCode<TGlobalOptions, TExitCode> : ICommandInterceptor
    where TGlobalOptions : class
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Intercepts command execution.
    /// </summary>
    /// <param name="context">The interceptor context with typed global options.</param>
    /// <param name="next">The next interceptor or the command execution delegate.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>The command exit code.</returns>
    Task<TExitCode> InterceptAsync(
        CommandInterceptorContext<TGlobalOptions> context,
        Func<CancellationToken, Task<TExitCode>> next,
        CancellationToken cancellationToken);

    async Task<int> ICommandInterceptor.InterceptAsync(
        CommandInterceptorContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken)
    {
        var result = await InterceptAsync(
            (CommandInterceptorContext<TGlobalOptions>)context,
            async token => (TExitCode)Enum.ToObject(
                typeof(TExitCode),
                await next.Invoke(token).ConfigureAwait(false)),
            cancellationToken).ConfigureAwait(false);

        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }
}
