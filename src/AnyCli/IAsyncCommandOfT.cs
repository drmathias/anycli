// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli;

/// <summary>
/// A command that executes asynchronously and returns a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
/// <remarks>
/// The enum value is converted to an <see cref="int"/> when the command completes.
/// </remarks>
public interface IAsyncCommand<TExitCode> : IAsyncCommand
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>A task that produces the command's exit code.</returns>
    new Task<TExitCode> ExecuteAsync(CommandContext context);

    /// <inheritdoc/>
    async Task<int> IAsyncCommand.ExecuteAsync(CommandContext context) =>
        Convert.ToInt32(await ExecuteAsync(context).ConfigureAwait(false), CultureInfo.InvariantCulture);
}
