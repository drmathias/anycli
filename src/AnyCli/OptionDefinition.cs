// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class OptionDefinition(
    string name,
    IReadOnlyList<string> aliases,
    string description,
    Type valueType,
    Type declaredType,
    bool required,
    bool takesValue,
    bool isCollection)
{
    public string Name { get; } = name;

    public IReadOnlyList<string> Aliases { get; } = aliases;

    public IReadOnlyList<string> Names { get; } = [name, .. aliases];

    public string Description { get; set; } = description;

    public Type ValueType { get; } = valueType;

    public Type DeclaredType { get; } = declaredType;

    public bool Required { get; } = required;

    public bool TakesValue { get; } = takesValue;

    public bool IsCollection { get; } = isCollection;
}
