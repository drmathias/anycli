// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

internal readonly record struct OptionBinding(OptionDefinition Definition, bool IsGlobal);

internal sealed class OptionLookup
{
    private readonly IReadOnlyDictionary<string, OptionBinding> _bindings;

    private OptionLookup(IReadOnlyDictionary<string, OptionBinding> bindings)
    {
        _bindings = bindings;
    }

    public static OptionLookup Empty { get; } = new(new Dictionary<string, OptionBinding>(StringComparer.Ordinal));

    public bool ContainsKey(string name) => _bindings.ContainsKey(name);

    public bool TryGetValue(string name, out OptionBinding binding) => _bindings.TryGetValue(name, out binding);

    public static OptionLookup Create(
        IReadOnlyList<OptionDefinition> optionDefinitions,
        IReadOnlyList<OptionDefinition> globalOptionDefinitions)
    {
        if (optionDefinitions.Count == 0 && globalOptionDefinitions.Count == 0) return Empty;

        var capacity = 0;
        foreach (var option in globalOptionDefinitions) capacity += option.Names.Count;
        foreach (var option in optionDefinitions) capacity += option.Names.Count;

        var bindings = new Dictionary<string, OptionBinding>(capacity, StringComparer.Ordinal);

        foreach (var option in globalOptionDefinitions)
        {
            foreach (var name in option.Names) bindings[name] = new OptionBinding(option, IsGlobal: true);
        }

        foreach (var option in optionDefinitions)
        {
            foreach (var name in option.Names) bindings[name] = new OptionBinding(option, IsGlobal: false);
        }

        return new OptionLookup(bindings);
    }
}
