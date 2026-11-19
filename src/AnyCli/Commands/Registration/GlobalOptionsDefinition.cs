// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Registration;

internal interface IGlobalOptionsDefinition
{
    CommandContext CreateContext(
        IReadOnlyList<string> commandPath,
        IConsole console,
        CommandOptionValues values);

    CommandInterceptorContext CreateInterceptorContext(
        CommandContext context,
        string rawCommand,
        IReadOnlyDictionary<string, object?> parsedArguments,
        IReadOnlyDictionary<string, object?> parsedOptions);
}

internal sealed class GlobalOptionsDefinition<TGlobalOptions> : IGlobalOptionsDefinition
    where TGlobalOptions : class, IGlobalOptions<TGlobalOptions>
{
    public CommandContext CreateContext(
        IReadOnlyList<string> commandPath,
        IConsole console,
        CommandOptionValues values) =>
        new CommandContext<TGlobalOptions>(
            commandPath,
            console,
            TGlobalOptions.Bind(values));

    public CommandInterceptorContext CreateInterceptorContext(
        CommandContext context,
        string rawCommand,
        IReadOnlyDictionary<string, object?> parsedArguments,
        IReadOnlyDictionary<string, object?> parsedOptions) =>
        new CommandInterceptorContext<TGlobalOptions>(
            (CommandContext<TGlobalOptions>)context,
            rawCommand,
            parsedArguments,
            parsedOptions);
}
