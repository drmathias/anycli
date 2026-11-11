// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Routing;

/// <summary>
/// The default handler for an unrecognized command.
/// </summary>
public sealed class DefaultUnrecognizedCommandHandler : IUnrecognizedCommandHandler
{
    /// <inheritdoc/>
    public ValueTask HandleAsync(
        UnrecognizedCommandContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Console.Error.WriteLine(string.IsNullOrEmpty(context.AttemptedCommandPath)
            ? "No command was specified."
            : context.IsMissingSubcommand
                ? $"Command '{context.AttemptedCommandPath}' requires a subcommand."
                : $"Unknown command '{context.AttemptedCommandPath}'.");

        return ValueTask.CompletedTask;
    }
}
