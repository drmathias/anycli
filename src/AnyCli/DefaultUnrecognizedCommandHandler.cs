// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// The default handler for an unrecognized command.
/// </summary>
public sealed class DefaultUnrecognizedCommandHandler : IUnrecognizedCommandHandler
{
    /// <inheritdoc/>
    public void Handle(UnrecognizedCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Console.Error.WriteLine(string.IsNullOrEmpty(context.CommandName)
            ? "No command was specified."
            : $"Unknown command '{context.CommandName}'.");
    }
}
