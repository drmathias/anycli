// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Commands.Registration;

internal sealed class CommandRegistration(
    Func<
        IServiceProvider?,
        CommandContext,
        ParsedCommandParameters,
        CommandOptionValues,
        CancellationToken,
        Task<int>> execute,
    IReadOnlyList<ArgumentDefinition> arguments,
    IReadOnlyList<OptionDefinition> options)
{
    public Func<
        IServiceProvider?,
        CommandContext,
        ParsedCommandParameters,
        CommandOptionValues,
        CancellationToken,
        Task<int>> Execute
    { get; } = execute;

    public IReadOnlyList<ArgumentDefinition> Arguments { get; } = arguments;

    public IReadOnlyList<OptionDefinition> Options { get; } = options;

    public Type? ParametersType { get; init; }
}
