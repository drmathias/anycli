// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal static class CommandArgumentsParser
{
    public static CommandArguments Parse(
        IReadOnlyList<ArgumentDefinition> definitions,
        IReadOnlyList<OptionDefinition> optionDefinitions,
        ReadOnlySpan<string> tokens)
    {
        var optionLookup = new Dictionary<string, OptionDefinition>(StringComparer.Ordinal);
        foreach (var option in optionDefinitions)
        {
            foreach (var name in option.Names)
            {
                optionLookup[name] = option;
            }
        }

        var optionValues = new Dictionary<string, OptionValue>(StringComparer.Ordinal);
        var positionalValues = new List<string>();
        var endOfOptions = false;

        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index];

            if (endOfOptions)
            {
                positionalValues.Add(token);
                continue;
            }

            if (token == "--")
            {
                endOfOptions = true;
                continue;
            }

            if (token.Length > 1 && token[0] == '-' && token[1] != '-')
            {
                index = ParseShortOption(
                    token,
                    tokens,
                    index,
                    optionLookup,
                    optionValues);
                continue;
            }

            if (token.Length > 2 && token[0] == '-' && token[1] == '-')
            {
                ParseLongOption(token, tokens, ref index, optionLookup, optionValues);
                continue;
            }

            positionalValues.Add(token);
        }

        foreach (var option in optionDefinitions)
        {
            if (option.Required && !optionValues.ContainsKey(option.Name))
            {
                throw new CommandArgumentException($"Missing required option '{option.Name}'.");
            }
        }

        var arities = new Dictionary<string, ArgumentArity>(StringComparer.Ordinal);
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        var supplied = new HashSet<string>(StringComparer.Ordinal);
        var position = 0;

        foreach (var definition in definitions)
        {
            arities[definition.Name] = definition.Arity;

            var available = positionalValues.Count - position;
            var count = definition.Arity.Maximum == int.MaxValue
                ? available
                : Math.Min(definition.Arity.Maximum, available);

            if (count < definition.Arity.Minimum)
            {
                throw new CommandArgumentException(
                    $"Missing required argument '{definition.Name}'.");
            }

            if (definition.Arity.IsVariadic)
            {
                var items = new List<string>(count);
                for (var itemIndex = 0; itemIndex < count; itemIndex++)
                {
                    items.Add(positionalValues[position++]);
                }

                values[definition.Name] = items;
            }
            else if (count > 0)
            {
                values[definition.Name] = positionalValues[position++];
                if (definition.Arity.Minimum == 0)
                {
                    supplied.Add(definition.Name);
                }
            }
            else
            {
                values[definition.Name] = definition.DefaultFactory();
            }
        }

        if (position < positionalValues.Count)
        {
            throw new CommandArgumentException(
                $"Unexpected argument '{positionalValues[position]}'.");
        }

        return new CommandArguments(
            arities,
            values,
            supplied,
            new CommandOptionValues(optionValues));
    }

    private static int ParseShortOption(
        string token,
        ReadOnlySpan<string> tokens,
        int index,
        IReadOnlyDictionary<string, OptionDefinition> optionLookup,
        IDictionary<string, OptionValue> optionValues)
    {
        var equalsIndex = token.IndexOf('=');
        if (equalsIndex >= 0)
        {
            var name = token[..equalsIndex];
            if (name.Length != 2)
            {
                throw new CommandArgumentException($"Invalid option '{name}'.");
            }

            if (!optionLookup.TryGetValue(name, out var option))
            {
                throw new CommandArgumentException($"Unknown option '{name}'.");
            }

            AddOptionValue(optionValues, option, token[(equalsIndex + 1)..]);
            return index;
        }

        if (token.Length == 2)
        {
            if (!optionLookup.TryGetValue(token, out var option))
            {
                throw new CommandArgumentException($"Unknown option '{token}'.");
            }

            if (option.TakesValue)
            {
                if (index + 1 >= tokens.Length || tokens[index + 1] == "--")
                {
                    throw new CommandArgumentException(
                        $"Missing value for option '{token}'.");
                }

                AddOptionValue(optionValues, option, tokens[++index]);
            }
            else
            {
                AddOptionValue(optionValues, option, null);
            }

            return index;
        }

        for (var characterIndex = 1; characterIndex < token.Length; characterIndex++)
        {
            var name = "-" + token[characterIndex];
            if (!optionLookup.TryGetValue(name, out var option))
            {
                throw new CommandArgumentException($"Unknown option '{name}'.");
            }

            if (option.TakesValue)
            {
                throw new CommandArgumentException(
                    $"Option '{name}' requires a separate value; use '{name} value' or '{name}=value'.");
            }

            AddOptionValue(optionValues, option, null);
        }

        return index;
    }

    private static void ParseLongOption(
        string token,
        ReadOnlySpan<string> tokens,
        ref int index,
        IReadOnlyDictionary<string, OptionDefinition> optionLookup,
        IDictionary<string, OptionValue> optionValues)
    {
        var equalsIndex = token.IndexOf('=');
        var name = equalsIndex >= 0 ? token[..equalsIndex] : token;
        if (!optionLookup.TryGetValue(name, out var option))
        {
            throw new CommandArgumentException($"Unknown option '{name}'.");
        }

        if (option.TakesValue)
        {
            if (equalsIndex >= 0)
            {
                AddOptionValue(optionValues, option, token[(equalsIndex + 1)..]);
                return;
            }

            if (index + 1 >= tokens.Length || tokens[index + 1] == "--")
            {
                throw new CommandArgumentException(
                    $"Missing value for option '{name}'.");
            }

            AddOptionValue(optionValues, option, tokens[++index]);
            return;
        }

        AddOptionValue(
            optionValues,
            option,
            equalsIndex >= 0 ? token[(equalsIndex + 1)..] : null);
    }

    private static void AddOptionValue(
        IDictionary<string, OptionValue> optionValues,
        OptionDefinition option,
        string? value)
    {
        if (!optionValues.TryGetValue(option.Name, out var optionValue))
        {
            optionValue = new OptionValue();
            optionValues[option.Name] = optionValue;
        }

        if (value is not null)
        {
            optionValue.Values.Add(value);
        }
    }
}
