// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class CommandRegistration(
    Func<IServiceProvider?, CommandContext, Task<int>> execute,
    IReadOnlyList<ArgumentDefinition> arguments)
{
    public Func<IServiceProvider?, CommandContext, Task<int>> Execute { get; } = execute;

    public IReadOnlyList<ArgumentDefinition> Arguments { get; } = arguments;
}
