// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters;

internal enum ParameterDefaultKind
{
    None,
    Constant,
    Provider,
}

internal sealed record ParameterDefault(
    ParameterDefaultKind Kind,
    object? Value,
    Type? ProviderType,
    HelpText DisplayText)
{
    public static ParameterDefault None { get; } = new(ParameterDefaultKind.None, null, null, HelpText.Empty);

    public bool IsDefined => Kind != ParameterDefaultKind.None;

    public bool IsProvider => Kind == ParameterDefaultKind.Provider;

    public ParameterDefault WithValue(object? value) => this with
    {
        Kind = ParameterDefaultKind.Constant,
        Value = value,
        ProviderType = null,
    };

    public ParameterDefault WithProvider(Type providerType) => this with
    {
        Kind = ParameterDefaultKind.Provider,
        Value = null,
        ProviderType = providerType,
    };

    public ParameterDefault WithDisplay(HelpText display) => this with { DisplayText = display };

    public DefaultValueInfo? ToInfo(bool hasInitializer) => IsProvider
            ? new DefaultValueInfo.Provider(ProviderType!) { DisplayText = DisplayText }
            : IsDefined
                ? new DefaultValueInfo.Initializer(Value) { DisplayText = DisplayText }
                : (DefaultValueInfo?)(hasInitializer
                    ? new DefaultValueInfo.Initializer(null) { DisplayText = DisplayText }
                    : null);
}
