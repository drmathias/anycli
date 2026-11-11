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
    /// <param name="context">The command context with typed global options.</param>
    /// <param name="next">The next interceptor or the command execution delegate.</param>
    /// <param name="cancellationToken">The cancellation token for the command.</param>
    /// <returns>The command exit code.</returns>
    Task<int> InterceptAsync(
        CommandContext<TGlobalOptions> context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken);

    Task<int> ICommandInterceptor.InterceptAsync(
        CommandContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken) =>
        InterceptAsync(
            (CommandContext<TGlobalOptions>)context,
            next,
            cancellationToken);
}
