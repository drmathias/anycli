// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Creates <see cref="IAnsiConsole"/> instances that write to AnyCli output writers.
/// </summary>
public static class SpectreConsoleAdapter
{
    private static readonly ConditionalWeakTable<CommandContext, IAnsiConsoleWriter> CommandContextConsoles = [];
    private static readonly ConditionalWeakTable<CommandErrorContext, IAnsiConsoleErrorWriter> CommandErrorContextConsoles = [];
    private static readonly ConditionalWeakTable<HelpContext, IAnsiConsoleWriter> HelpContextConsoles = [];

    /// <summary>
    /// Creates an ANSI console that writes to the specified output writer.
    /// </summary>
    /// <param name="writer">The output writer to write to.</param>
    /// <param name="options">The console options, or <see langword="null"/> to use defaults.</param>
    /// <returns>A new <see cref="IAnsiConsole"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="writer"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Raw <see cref="IOutputWriter.Write(string)"/> and
    /// <see cref="IOutputWriter.WriteLine(string)"/> calls still write directly to
    /// <paramref name="writer"/>, so they can be mixed with ANSI output.
    /// </remarks>
    public static IAnsiConsole Create(IOutputWriter writer, SpectreConsoleOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(writer);
        options ??= new SpectreConsoleOptions();

        return AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = options.Ansi,
            ColorSystem = options.ColorSystem,
            Interactive = options.Interactive,
            Out = new AnsiConsoleOutput(new OutputWriterTextWriter(writer)),
        });
    }

    /// <summary>
    /// Creates an ANSI console that writes to the standard output of a console.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="options">The console options, or <see langword="null"/> to use defaults.</param>
    /// <returns>A new <see cref="IAnsiConsole"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="console"/> is <see langword="null"/>.
    /// </exception>
    public static IAnsiConsole Create(IConsole console, SpectreConsoleOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(console);
        return Create(console.Output, options);
    }

    /// <summary>
    /// Creates an ANSI console that writes to the standard error of a console.
    /// </summary>
    /// <param name="console">The console.</param>
    /// <param name="options">The console options, or <see langword="null"/> to use defaults.</param>
    /// <returns>A new <see cref="IAnsiConsole"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="console"/> is <see langword="null"/>.
    /// </exception>
    public static IAnsiConsole CreateError(IConsole console, SpectreConsoleOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(console);
        return Create(console.Error, options);
    }

    extension(CommandContext context)
    {
        /// <summary>
        /// Gets the cached ANSI consoles for the command's standard output and error.
        /// </summary>
        /// <remarks>
        /// The consoles are created once for the command context and reused on later accesses.
        /// </remarks>
        public IAnsiConsoleWriter AnsiConsole => CommandContextConsoles.GetValue(context, static commandContext => CreateConsoles(commandContext.Console));
    }

    extension(HelpContext context)
    {
        /// <summary>
        /// Gets the cached ANSI consoles for the help context's standard output and error.
        /// </summary>
        /// <remarks>
        /// The consoles are created once for the help context and reused on later accesses.
        /// </remarks>
        public IAnsiConsoleWriter AnsiConsole => HelpContextConsoles.GetValue(context, static helpContext => CreateConsoles(helpContext.Console));
    }

    extension(CommandErrorContext context)
    {
        /// <summary>
        /// Gets the cached ANSI console for the error context's standard error.
        /// </summary>
        /// <remarks>
        /// The console is created once for the context and reused on later accesses.
        /// </remarks>
        public IAnsiConsoleErrorWriter AnsiConsole => CommandErrorContextConsoles.GetValue(
            context,
            static errorContext => new AnsiConsoleErrorWriter(
                Create(errorContext.Console.Error)));
    }

    private static AnsiConsoleWriter CreateConsoles(IConsoleWriter console) => new(Create(console.Output), Create(console.Error));
}
