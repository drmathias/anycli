// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal enum ArgumentKind
{
    Required,
    Optional,
    Variadic,
}

internal sealed class ArgumentDefinition(string name, ArgumentKind kind, Func<string?> defaultFactory)
{
    public string Name { get; } = name;

    public ArgumentKind Kind { get; } = kind;

    public Func<string?> DefaultFactory { get; } = defaultFactory;

    public string Description { get; set; } = string.Empty;
}
