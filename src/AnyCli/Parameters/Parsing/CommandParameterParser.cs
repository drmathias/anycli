// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

internal static class CommandParameterParser
{
    public static int ParseLeadingGlobalOptions(
        OptionLookup optionLookup,
        ReadOnlySpan<string> tokens,
        IDictionary<string, OptionValue> globalValues,
        bool hasHelp,
        bool hasVersion,
        out bool helpRequested,
        out bool versionRequested)
    {
        helpRequested = false;
        versionRequested = false;

        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index];

            if (token == "--")
            {
                return index + 1 < tokens.Length ? index + 1 : -1;
            }

            if (hasHelp && IsHelpFlag(token))
            {
                helpRequested = true;
                continue;
            }

            if (hasVersion && IsVersionFlag(token))
            {
                versionRequested = true;
                continue;
            }

            if (token.Length > 1 && token[0] == '-' && token[1] != '-')
            {
                index = ParseShortOption(
                    token,
                    tokens,
                    index,
                    optionLookup,
                    commandValues: null,
                    globalValues);
                continue;
            }

            if (token.Length > 2 && token[0] == '-' && token[1] == '-')
            {
                ParseLongOption(
                    token,
                    tokens,
                    ref index,
                    optionLookup,
                    commandValues: null,
                    globalValues);
                continue;
            }

            return index;
        }

        return -1;
    }

    public static int FindFirstNonGlobalOption(
        OptionLookup optionLookup,
        ReadOnlySpan<string> tokens,
        IDictionary<string, OptionValue> globalValues,
        bool hasHelp,
        bool hasVersion,
        out bool helpRequested,
        out bool versionRequested)
    {
        helpRequested = false;
        versionRequested = false;

        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index];

            if (token == "--")
            {
                return index;
            }

            if (hasHelp && IsHelpFlag(token))
            {
                helpRequested = true;
                continue;
            }

            if (hasVersion && IsVersionFlag(token))
            {
                versionRequested = true;
                continue;
            }

            if (token.Length > 1 && token[0] == '-' && token[1] != '-')
            {
                if (!IsShortGlobalOption(token, optionLookup))
                {
                    return index;
                }

                index = ParseShortOption(
                    token,
                    tokens,
                    index,
                    optionLookup,
                    commandValues: null,
                    globalValues);
                continue;
            }

            if (token.Length > 2 && token[0] == '-' && token[1] == '-')
            {
                if (!IsLongGlobalOption(token, optionLookup))
                {
                    return index;
                }

                ParseLongOption(
                    token,
                    tokens,
                    ref index,
                    optionLookup,
                    commandValues: null,
                    globalValues);
                continue;
            }

            return index;
        }

        return -1;
    }

    public static ParsedCommandParameters? Parse(
        IReadOnlyList<ArgumentDefinition> definitions,
        IReadOnlyList<OptionDefinition> optionDefinitions,
        OptionLookup optionLookup,
        ReadOnlySpan<string> tokens,
        IDictionary<string, OptionValue> globalValues,
        bool hasHelp,
        bool hasVersion,
        out bool helpRequested,
        out bool versionRequested)
    {
        var commandOptionValues = new Dictionary<string, OptionValue>(
            optionDefinitions.Count,
            StringComparer.Ordinal);
        var positionalValues = new List<string>();
        var endOfOptions = false;

        helpRequested = false;
        versionRequested = false;

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

            if (hasHelp && IsHelpFlag(token))
            {
                helpRequested = true;
                return null;
            }

            if (hasVersion && IsVersionFlag(token))
            {
                versionRequested = true;
                return null;
            }

            if (token.Length > 1 && token[0] == '-' && token[1] != '-')
            {
                index = ParseShortOption(
                    token,
                    tokens,
                    index,
                    optionLookup,
                    commandOptionValues,
                    globalValues);
                continue;
            }

            if (token.Length > 2 && token[0] == '-' && token[1] == '-')
            {
                ParseLongOption(
                    token,
                    tokens,
                    ref index,
                    optionLookup,
                    commandOptionValues,
                    globalValues);
                continue;
            }

            positionalValues.Add(token);
        }

        foreach (var option in optionDefinitions)
        {
            if (option.Required && !commandOptionValues.ContainsKey(option.Name))
            {
                throw new CommandErrorException(
                    new MissingRequiredOptionError(option.Name, IsGlobal: false));
            }
        }

        var arities = new Dictionary<string, ArgumentArity>(
            definitions.Count,
            StringComparer.Ordinal);
        var values = new Dictionary<string, object?>(
            definitions.Count,
            StringComparer.Ordinal);
        HashSet<string>? supplied = null;
        var position = 0;

        foreach (var definition in definitions)
        {
            arities[definition.Name] = definition.Arity;

            var available = positionalValues.Count - position;

            if (definition.Arity.IsVariadic)
            {
                if (available < definition.Arity.Minimum)
                {
                    throw new CommandErrorException(
                        new TooFewVariadicValuesError(
                            definition.Name,
                            definition.Arity.Minimum,
                            available));
                }

                if (definition.Arity.Maximum != int.MaxValue &&
                    available > definition.Arity.Maximum)
                {
                    throw new CommandErrorException(
                        new TooManyVariadicValuesError(
                            definition.Name,
                            definition.Arity.Maximum,
                            available));
                }
            }

            var count = definition.Arity.Maximum == int.MaxValue
                ? available
                : Math.Min(definition.Arity.Maximum, available);

            if (count < definition.Arity.Minimum)
            {
                throw new CommandErrorException(
                    new MissingRequiredArgumentError(definition.Name));
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
                    supplied ??= new HashSet<string>(StringComparer.Ordinal);
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
            throw new CommandErrorException(
                new UnexpectedArgumentError(positionalValues[position]));
        }

        return new ParsedCommandParameters(
            new CommandArgumentValues(arities, values, supplied),
            new CommandOptionValues(commandOptionValues));
    }

    private static bool IsShortGlobalOption(
        string token,
        OptionLookup optionLookup)
    {
        var equalsIndex = token.IndexOf('=');
        if (equalsIndex >= 0)
        {
            var name = token[..equalsIndex];
            return name.Length == 2 && optionLookup.ContainsKey(name);
        }

        if (token.Length == 2)
        {
            return optionLookup.ContainsKey(token);
        }

        for (var index = 1; index < token.Length; index++)
        {
            if (!optionLookup.TryGetValue("-" + token[index], out var option) ||
                option.Definition.TakesValue)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsLongGlobalOption(
        string token,
        OptionLookup optionLookup)
    {
        var equalsIndex = token.IndexOf('=');
        var name = equalsIndex >= 0 ? token[..equalsIndex] : token;
        return optionLookup.ContainsKey(name);
    }

    private static bool IsHelpFlag(string token) => token is "-h" or "--help";

    private static bool IsVersionFlag(string token) => token is "-v" or "--version";

    private static int ParseShortOption(
        string token,
        ReadOnlySpan<string> tokens,
        int index,
        OptionLookup optionLookup,
        IDictionary<string, OptionValue>? commandValues,
        IDictionary<string, OptionValue> globalValues)
    {
        var equalsIndex = token.IndexOf('=');
        if (equalsIndex >= 0)
        {
            var name = token[..equalsIndex];
            if (name.Length != 2)
            {
                throw new CommandErrorException(new MalformedOptionError(name));
            }

            if (!optionLookup.TryGetValue(name, out var binding))
            {
                throw new CommandErrorException(new UnknownOptionError(name));
            }

            AddOptionValue(
                commandValues,
                globalValues,
                binding,
                token[(equalsIndex + 1)..]);
            return index;
        }

        if (token.Length == 2)
        {
            if (!optionLookup.TryGetValue(token, out var binding))
            {
                throw new CommandErrorException(new UnknownOptionError(token));
            }

            if (binding.Definition.TakesValue)
            {
                if (index + 1 >= tokens.Length || tokens[index + 1] == "--")
                {
                    throw new CommandErrorException(new MissingOptionValueError(token));
                }

                AddOptionValue(commandValues, globalValues, binding, tokens[++index]);
            }
            else
            {
                AddOptionValue(commandValues, globalValues, binding, null);
            }

            return index;
        }

        for (var characterIndex = 1; characterIndex < token.Length; characterIndex++)
        {
            var name = "-" + token[characterIndex];
            if (!optionLookup.TryGetValue(name, out var binding))
            {
                throw new CommandErrorException(new UnknownOptionError(name));
            }

            if (binding.Definition.TakesValue)
            {
                throw new CommandErrorException(new OptionRequiresSeparateValueError(name));
            }

            AddOptionValue(commandValues, globalValues, binding, null);
        }

        return index;
    }

    private static void ParseLongOption(
        string token,
        ReadOnlySpan<string> tokens,
        ref int index,
        OptionLookup optionLookup,
        IDictionary<string, OptionValue>? commandValues,
        IDictionary<string, OptionValue> globalValues)
    {
        var equalsIndex = token.IndexOf('=');
        var name = equalsIndex >= 0 ? token[..equalsIndex] : token;
        if (!optionLookup.TryGetValue(name, out var binding))
        {
            throw new CommandErrorException(new UnknownOptionError(name));
        }

        if (binding.Definition.TakesValue)
        {
            if (equalsIndex >= 0)
            {
                AddOptionValue(
                    commandValues,
                    globalValues,
                    binding,
                    token[(equalsIndex + 1)..]);
                return;
            }

            if (index + 1 >= tokens.Length || tokens[index + 1] == "--")
            {
                throw new CommandErrorException(new MissingOptionValueError(name));
            }

            AddOptionValue(commandValues, globalValues, binding, tokens[++index]);
            return;
        }

        AddOptionValue(
            commandValues,
            globalValues,
            binding,
            equalsIndex >= 0 ? token[(equalsIndex + 1)..] : null);
    }

    private static void AddOptionValue(
        IDictionary<string, OptionValue>? commandValues,
        IDictionary<string, OptionValue> globalValues,
        OptionBinding binding,
        string? value)
    {
        var optionValues = binding.IsGlobal
            ? globalValues
            : commandValues ?? throw new InvalidOperationException(
                "Command options are not supported during global option parsing.");
        if (optionValues.TryGetValue(binding.Definition.Name, out var optionValue))
        {
            if (!binding.Definition.IsCollection)
            {
                throw new CommandErrorException(
                    new RepeatedOptionError(binding.Definition.Name));
            }
        }
        else
        {
            optionValue = new OptionValue();
            optionValues[binding.Definition.Name] = optionValue;
        }

        if (value is not null)
        {
            optionValue.Add(value, binding.Definition.IsCollection);
        }
    }
}
