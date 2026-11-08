// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli;

internal sealed class CommandInterceptorAdapter<TExitCode>(
    ICommandInterceptor<TExitCode> interceptor) : ICommandInterceptor
    where TExitCode : struct, Enum
{
    public async Task<int> InterceptAsync(CommandContext context, Func<Task<int>> next)
    {
        var result = await interceptor.InterceptAsync(
            context,
            async () => (TExitCode)Enum.ToObject(
                typeof(TExitCode),
                await next().ConfigureAwait(false))).ConfigureAwait(false);

        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }
}
