// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Interceptors;

/// <summary>
/// Intercepts command execution with typed global options.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
public interface ICommandInterceptor<TGlobalOptions> : ICommandInterceptor
    where TGlobalOptions : class
{
    /// <summary>
    /// Intercepts command execution.
    /// </summary>
    /// <param name="context">The interceptor context with typed global options.</param>
    /// <param name="next">The next interceptor or the command execution delegate.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>The command exit code.</returns>
    Task<int> InterceptAsync(
        CommandInterceptorContext<TGlobalOptions> context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken);

    Task<int> ICommandInterceptor.InterceptAsync(
        CommandInterceptorContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken) =>
        InterceptAsync(
            (CommandInterceptorContext<TGlobalOptions>)context,
            next,
            cancellationToken);
}
