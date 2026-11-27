// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Interceptors;

/// <summary>
/// Provides command execution information to interceptors.
/// </summary>
public class CommandInterceptorContext : CommandContext
{
    internal CommandInterceptorContext(
        CommandContext context,
        string rawCommand,
        IReadOnlyDictionary<string, ParsedValue> parsedArguments,
        IReadOnlyDictionary<string, ParsedValue> parsedOptions)
        : base(context.CommandPath, context.Console, context.Application)
    {
        RawCommand = rawCommand;
        ParsedArguments = parsedArguments;
        ParsedOptions = parsedOptions;
    }

    /// <summary>
    /// Gets the raw command line as a display string.
    /// </summary>
    public string RawCommand { get; }

    /// <summary>
    /// Gets the parsed positional argument values.
    /// </summary>
    public IReadOnlyDictionary<string, ParsedValue> ParsedArguments { get; }

    /// <summary>
    /// Gets the parsed option values.
    /// </summary>
    public IReadOnlyDictionary<string, ParsedValue> ParsedOptions { get; }
}

/// <summary>
/// Provides command execution information and typed global options to interceptors.
/// </summary>
/// <typeparam name="TGlobalOptions">The global options type.</typeparam>
public sealed class CommandInterceptorContext<TGlobalOptions> : CommandInterceptorContext
    where TGlobalOptions : class
{
    internal CommandInterceptorContext(
        CommandContext<TGlobalOptions> context,
        string rawCommand,
        IReadOnlyDictionary<string, ParsedValue> parsedArguments,
        IReadOnlyDictionary<string, ParsedValue> parsedOptions)
        : base(context, rawCommand, parsedArguments, parsedOptions)
    {
        GlobalOptions = context.GlobalOptions;
    }

    /// <summary>
    /// Gets the parsed global options.
    /// </summary>
    public TGlobalOptions GlobalOptions { get; }
}
