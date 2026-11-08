// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class ArgumentDefinition(
    string name,
    ArgumentArity arity,
    Type argumentType,
    Func<string?> defaultFactory)
{
    public string Name { get; } = name;

    public ArgumentArity Arity { get; } = arity;

    public Type DeclaredType { get; } = argumentType;

    public Func<string?> DefaultFactory { get; } = defaultFactory;

    public string Description { get; set; } = string.Empty;

    public object? DefaultValue { get; private set; }

    public bool HasDefaultValue { get; private set; }

    public void SetDefaultValue(object? value)
    {
        DefaultValue = value;
        HasDefaultValue = true;
    }
}
