// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// The default Spectre.Console handler for an unrecognized command.
/// </summary>
public sealed class DefaultSpectreUnrecognizedCommandHandler : IUnrecognizedCommandHandler
{
    /// <inheritdoc/>
    public ValueTask HandleAsync(
        UnrecognizedCommandContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        var message = string.IsNullOrEmpty(context.AttemptedCommandPath)
            ? "No command was specified."
            : context.IsMissingSubcommand
                ? $"Command '{context.AttemptedCommandPath}' requires a subcommand."
                : $"Unknown command '{context.AttemptedCommandPath}'.";

        context.AnsiConsole.Error.MarkupLineInterpolated($"[red]{Markup.Escape(message)}[/]");

        return ValueTask.CompletedTask;
    }
}
