// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class CommandRegistration(
    Func<IServiceProvider?, CommandContext, ParsedCommandParameters, Task<int>> execute,
    IReadOnlyList<ArgumentDefinition> arguments,
    IReadOnlyList<OptionDefinition> options)
{
    public Func<IServiceProvider?, CommandContext, ParsedCommandParameters, Task<int>> Execute { get; } = execute;

    public IReadOnlyList<ArgumentDefinition> Arguments { get; } = arguments;

    public IReadOnlyList<OptionDefinition> Options { get; } = options;
}
