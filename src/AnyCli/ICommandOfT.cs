// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace AnyCli;

/// <summary>
/// A command that executes synchronously and returns a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
/// <remarks>
/// The enum value is converted to an <see cref="int"/>.
/// </remarks>
public interface ICommand<TExitCode> : ICommand
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <returns>The command's exit code.</returns>
    new TExitCode Execute(CommandContext context);

    /// <inheritdoc/>
    int ICommand.Execute(CommandContext context) => Convert.ToInt32(Execute(context), CultureInfo.InvariantCulture);
}
