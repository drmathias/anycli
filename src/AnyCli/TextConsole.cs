// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// An <see cref="IConsole"/> backed by text readers and writers.
/// </summary>
public sealed class TextConsole : IConsole
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextConsole"/> class.
    /// </summary>
    /// <param name="input">The reader used for standard input.</param>
    /// <param name="output">The writer used for standard output.</param>
    /// <param name="error">The writer used for standard error.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="input"/>, <paramref name="output"/>, or <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public TextConsole(TextReader input, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        Input = new TextReaderInputReader(input);
        Output = new TextWriterOutputWriter(output);
        Error = new TextWriterOutputWriter(error);
    }

    /// <inheritdoc/>
    public IOutputWriter Output { get; }

    /// <inheritdoc/>
    public IOutputWriter Error { get; }

    /// <inheritdoc/>
    public IInputReader Input { get; }
}
