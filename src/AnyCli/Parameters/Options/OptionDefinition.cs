// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

internal sealed class OptionDefinition(
    string name,
    IReadOnlyList<string> aliases,
    HelpText description,
    Type valueType,
    Type declaredType,
    bool required,
    OptionValueKind valueKind)
{
    public string Name { get; } = name;

    public IReadOnlyList<string> Aliases { get; private set; } = aliases;

    public IReadOnlyList<string> Names { get; private set; } = [name, .. aliases];

    public HelpText Description { get; set; } = description;

    public Type ValueType { get; } = valueType;

    public Type DeclaredType { get; } = declaredType;

    public bool Required { get; set; } = required;

    public OptionValueKind ValueKind { get; } = valueKind;

    private ParameterDefault _default = ParameterDefault.None;

    public object? DefaultValue => _default.Value;

    public bool HasDefaultValue => _default.IsDefined;

    public bool HasInitializer { get; private set; }

    public bool HasDefaultValueSource => HasDefaultValue || HasInitializer;

    public Type? DefaultValueProviderType => _default.ProviderType;

    public HelpText DefaultValueDisplay => _default.DisplayText;

    internal DefaultValueInfo? GetDefaultValueInfo() => _default.ToInfo(HasInitializer);

    public void SetDefaultValue(object? value)
    {
        _default = _default.WithValue(value);
    }

    public void SetDefaultValueProvider(Type providerType)
    {
        ArgumentNullException.ThrowIfNull(providerType);
        _default = _default.WithProvider(providerType);
    }

    public void SetDefaultValueDisplay(HelpText display)
    {
        _default = _default.WithDisplay(display);
    }

    public void SetHasInitializer()
    {
        HasInitializer = true;
    }

    public void SetAliases(IReadOnlyList<string> aliases)
    {
        Aliases = aliases;
        Names = [Name, .. aliases];
    }
}
