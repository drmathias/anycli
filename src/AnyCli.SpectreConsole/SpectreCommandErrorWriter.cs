// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Writes command-line errors using Spectre.Console styling to standard error.
/// </summary>
/// <param name="options">
/// The error output options, or <see langword="null"/> to use defaults.
/// </param>
public sealed class SpectreCommandErrorWriter(SpectreErrorOptions? options = null) : ICommandErrorWriter
{
    private readonly SpectreErrorOptions _options = options ?? new SpectreErrorOptions();

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectreCommandErrorWriter"/> class.
    /// </summary>
    public SpectreCommandErrorWriter() : this(null)
    {
    }

    /// <inheritdoc/>
    public ValueTask WriteAsync(
        CommandErrorContext context,
        string message,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var console = _options.ConsoleOptions is null
            ? context.AnsiConsole.Error
            : SpectreConsoleAdapter.Create(context.Console.Error, _options.ConsoleOptions);

        console.Write(new Text(message, _options.ErrorStyle));
        console.WriteLine();

        return ValueTask.CompletedTask;
    }
}
