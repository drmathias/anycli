// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// The default command-line error handler.
/// </summary>
/// <param name="messageProvider">The provider for error messages.</param>
/// <param name="writer">The writer for standard error.</param>
/// <param name="exitCode">The exit code returned after handling an error.</param>
public sealed class DefaultCommandErrorHandler(
    ICommandErrorMessageProvider messageProvider,
    ICommandErrorWriter writer,
    int exitCode = 2) : ICommandErrorHandler
{
    /// <inheritdoc/>
    public async ValueTask<int> HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(messageProvider);
        ArgumentNullException.ThrowIfNull(writer);

        var message = messageProvider.GetMessage(context);
        await writer.WriteAsync(context, message, cancellationToken).ConfigureAwait(false);
        return exitCode;
    }
}

/// <summary>
/// The default command-line error handler with a custom exit code enum.
/// </summary>
/// <typeparam name="TExitCode">The exit code enum type.</typeparam>
/// <param name="messageProvider">The provider for error messages.</param>
/// <param name="writer">The writer for standard error.</param>
/// <param name="exitCode">The exit code returned after handling an error.</param>
public sealed class DefaultCommandErrorHandler<TExitCode>(
    ICommandErrorMessageProvider messageProvider,
    ICommandErrorWriter writer,
    TExitCode exitCode) : ICommandErrorHandler<TExitCode>
    where TExitCode : struct, Enum
{
    /// <inheritdoc/>
    public async ValueTask<TExitCode> HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(messageProvider);
        ArgumentNullException.ThrowIfNull(writer);

        var message = messageProvider.GetMessage(context);
        await writer.WriteAsync(context, message, cancellationToken).ConfigureAwait(false);
        return exitCode;
    }
}
