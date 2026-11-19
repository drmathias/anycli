<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Parse custom value types

Use a custom parser when an argument or option value is not covered by the built-in scalar types.

## Implement a parser

Implement `IParameterParser<T>` for the value type:

```csharp
using System.Globalization;

using AnyCli.Parameters.Parsing;

public readonly record struct TicketId(int Value);

public sealed class TicketIdParser : IParameterParser<TicketId>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out TicketId result)
    {
        if (int.TryParse(value, NumberStyles.None, provider, out var id) && id > 0)
        {
            result = new TicketId(id);
            return true;
        }

        result = default;
        return false;
    }
}
```

## Use the parser

Set `Parser` on the argument or option attribute:

```csharp
public sealed partial record InspectParameters
{
    [Argument(0, Parser = typeof(TicketIdParser))]
    public TicketId Id { get; set; }
}

public sealed partial record CloseParameters
{
    [Option("--ticket", Parser = typeof(TicketIdParser))]
    public TicketId Ticket { get; set; }
}
```

Invalid values produce a structured command-line error and return exit code `2` by default.

## Reuse parsers for a type

Register a parser once for a value type with the assembly-level attribute:

```csharp
[assembly: ParameterValueParser<TicketId, TicketIdParser>]
```

Parameters of `TicketId` then use the parser without setting `Parser` on each attribute.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Command-line syntax](../reference/command-line-syntax.md)
