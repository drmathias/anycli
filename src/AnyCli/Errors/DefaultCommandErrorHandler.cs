// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// The default command-line error handler.
/// </summary>
/// <param name="messageProvider">The provider for error messages.</param>
/// <param name="writer">The writer for standard error.</param>
public sealed class DefaultCommandErrorHandler(
    ICommandErrorMessageProvider messageProvider,
    ICommandErrorWriter writer) : ICommandErrorHandler
{
    /// <inheritdoc/>
    public async ValueTask HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(messageProvider);
        ArgumentNullException.ThrowIfNull(writer);

        var message = messageProvider.GetMessage(context);
        await writer.WriteAsync(context, message, cancellationToken).ConfigureAwait(false);
    }
}
