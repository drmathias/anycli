<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Parse custom value types

Use this guide when a command parameter type is not already parsed by AnyCLI.

If you control the type, prefer implementing `IParsable<T>` or `ISpanParsable<T>` before writing a separate parser. AnyCLI discovers those interfaces automatically.

## Use a standard parsing interface

For a type you own, implement `IParsable<T>`:

```csharp
using System.Globalization;

public readonly record struct Slug(string Value) : IParsable<Slug>
{
    public static Slug Parse(string value, IFormatProvider? provider) =>
        TryParse(value, provider, out var result)
            ? result
            : throw new FormatException($"Invalid slug '{value}'.");

    public static bool TryParse(
        string? value,
        IFormatProvider? provider,
        out Slug result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            return false;
        }

        result = new Slug(value.Trim().ToLowerInvariant());
        return true;
    }
}
```

Use the type directly in a parameter:

```csharp
public sealed partial record CreateParameters
{
    [Argument(0)]
    public required Slug Slug { get; init; }
}
```

Implement `ISpanParsable<T>` instead when the type can use span-based parsing. Because it extends `IParsable<T>`, provide both the string and span members. AnyCLI uses the span implementation automatically.

## Use a custom parser for types you do not control

If the type is external, or you need different parsing rules, implement `IParameterParser<T>`:

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

## Apply the parser

Set `Parser` on the specific argument or option:

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

## Register a parser once

Register the parser for the value type when it should apply everywhere:

```csharp
[assembly: ParameterValueParser<TicketId, TicketIdParser>]
```

Parameters of `TicketId` then use the parser without setting `Parser` on each attribute. A type can have only one registered parser.

## Related

- [Parameter parsing reference](../reference/parameter-parsing.md)
- [Diagnostics reference](../reference/diagnostics.md)
- [Parameter attributes reference](../reference/parameter-attributes.md)
