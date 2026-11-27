// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Registration;

internal interface IGlobalOptionsDefinition
{
    ValueTask<CommandContext> CreateContextAsync(
        IReadOnlyList<string> commandPath,
        IConsole console,
        CommandOptionValues values,
        ApplicationInfo application,
        CancellationToken cancellationToken,
        IServiceProvider? services);

    CommandInterceptorContext CreateInterceptorContext(
        CommandContext context,
        string rawCommand,
        IReadOnlyDictionary<string, ParsedValue> parsedArguments,
        IReadOnlyDictionary<string, ParsedValue> parsedOptions);
}

internal sealed class GlobalOptionsDefinition<TGlobalOptions> : IGlobalOptionsDefinition
    where TGlobalOptions : class, IGlobalOptions<TGlobalOptions>
{
    public async ValueTask<CommandContext> CreateContextAsync(
        IReadOnlyList<string> commandPath,
        IConsole console,
        CommandOptionValues values,
        ApplicationInfo application,
        CancellationToken cancellationToken,
        IServiceProvider? services)
    {
        var globalOptions = await TGlobalOptions.BindAsync(
                values,
                cancellationToken,
                services)
            .ConfigureAwait(false);
        values.BoundGlobalOptions = globalOptions;
        return new CommandContext<TGlobalOptions>(commandPath, console, globalOptions, application);
    }

    public CommandInterceptorContext CreateInterceptorContext(
        CommandContext context,
        string rawCommand,
        IReadOnlyDictionary<string, ParsedValue> parsedArguments,
        IReadOnlyDictionary<string, ParsedValue> parsedOptions) =>
        new CommandInterceptorContext<TGlobalOptions>(
            (CommandContext<TGlobalOptions>)context,
            rawCommand,
            parsedArguments,
            parsedOptions);
}
