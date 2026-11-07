// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal static class CommandArgumentsParser
{
    public static CommandArguments Parse(
        IReadOnlyList<ArgumentDefinition> definitions,
        ReadOnlySpan<string> tokens)
    {
        var arities = new Dictionary<string, ArgumentArity>(StringComparer.Ordinal);
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        var supplied = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;

        foreach (var definition in definitions)
        {
            arities[definition.Name] = definition.Arity;

            var available = tokens.Length - index;
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
                    items.Add(tokens[index++]);
                }

                values[definition.Name] = items;
            }
            else if (count > 0)
            {
                values[definition.Name] = tokens[index++];
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

        return index < tokens.Length
            ? throw new CommandArgumentException($"Unexpected argument '{tokens[index]}'.")
            : new CommandArguments(arities, values, supplied);
    }
}
