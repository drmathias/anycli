// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Arguments;

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

    public HelpText Description { get; set; } = HelpText.Empty;

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
}
