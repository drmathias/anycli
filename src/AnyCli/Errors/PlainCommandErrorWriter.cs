// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Writes command-line errors as plain text without styling.
/// </summary>
public sealed class PlainCommandErrorWriter : ICommandErrorWriter
{
    /// <inheritdoc/>
    public ValueTask WriteAsync(
        CommandErrorContext context,
        string message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        context.Console.Error.WriteLine(message);
        return ValueTask.CompletedTask;
    }
}
