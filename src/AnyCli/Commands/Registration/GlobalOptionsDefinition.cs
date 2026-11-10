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
}
