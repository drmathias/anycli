// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal static class CommandArgumentsParser
{
    public static CommandArguments Parse(
        IReadOnlyList<ArgumentDefinition> definitions,
        ReadOnlySpan<string> tokens)
    {
        var kinds = new Dictionary<string, ArgumentKind>(StringComparer.Ordinal);
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        var supplied = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;

        foreach (var definition in definitions)
        {
            kinds[definition.Name] = definition.Kind;

            switch (definition.Kind)
            {
                case ArgumentKind.Required:
                    if (index >= tokens.Length)
                    {
                        throw new CommandArgumentException(
                            $"Missing required argument '{definition.Name}'.");
                    }

                    values[definition.Name] = tokens[index++];
                    break;

                case ArgumentKind.Optional:
                    if (index < tokens.Length)
                    {
                        values[definition.Name] = tokens[index++];
                        supplied.Add(definition.Name);
                    }
                    else
                    {
                        values[definition.Name] = definition.DefaultFactory();
                    }

                    break;

                case ArgumentKind.Variadic:
                    var items = new List<string>();
                    while (index < tokens.Length)
                    {
                        items.Add(tokens[index++]);
                    }

                    values[definition.Name] = items;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown argument kind '{definition.Kind}'.");
            }
        }

        if (index < tokens.Length)
        {
            throw new CommandArgumentException($"Unexpected argument '{tokens[index]}'.");
        }

        return new CommandArguments(kinds, values, supplied);
    }
}
