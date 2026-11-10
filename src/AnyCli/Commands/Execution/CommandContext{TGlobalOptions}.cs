// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Execution;

/// <summary>
/// Provides the runtime context for a command execution with typed global options.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
public sealed class CommandContext<TGlobalOptions> : CommandContext
    where TGlobalOptions : class
{
    internal CommandContext(
        IReadOnlyList<string> commandPath,
        IConsole console,
        TGlobalOptions globalOptions)
        : base(commandPath, console)
    {
        GlobalOptions = globalOptions;
    }

    /// <summary>
    /// Gets the parsed global options that apply to every command.
    /// </summary>
    public TGlobalOptions GlobalOptions { get; }
}
