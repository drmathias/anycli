// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli;

public interface IAsyncCommand<TExitCode> : IAsyncCommand
    where TExitCode : struct, Enum
{
    new Task<TExitCode> ExecuteAsync(CommandContext context);

    async Task<int> IAsyncCommand.ExecuteAsync(CommandContext context) => Convert.ToInt32(await ExecuteAsync(context).ConfigureAwait(false), CultureInfo.InvariantCulture);
}
