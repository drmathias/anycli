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
    public void Handle(UnrecognizedCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var message = string.IsNullOrEmpty(context.CommandName)
            ? "No command was specified."
            : $"Unknown command '{context.CommandName}'.";

        context.AnsiConsole.Error.MarkupLineInterpolated($"[red]{Markup.Escape(message)}[/]");
    }
}
