<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Parameter parsing reference

AnyCLI resolves a parser for every argument and option value at compile time. It does not use `TypeConverter` or a convention-based `TryParse` method. This page describes precedence and parsing behavior. For exact contracts and signatures, use the API Reference in the top navigation.

## Resolution order

| Priority | Rule | Notes |
| ---: | --- | --- |
| 1 | Explicit `Parser` on the argument or option | Highest precedence. |
| 2 | Registered `ParameterValueParserAttribute<TValue, TParser>` | One parser per type. |
| 3 | `string` | Passed through without conversion. |
| 4 | Enum | Default enum parser or a registered `EnumParameterParserAttribute<TParser>`. |
| 5 | `ISpanParsable<T>` | Uses the span-based `TryParse`. |
| 6 | `IParsable<T>` | Uses the string-based `TryParse`. |
| 7 | Unsupported | Source generator emits [`ANYCLI001`](diagnostics.md). |

Explicit and registered parsers override enum handling for a specific enum type.

The default enum parser matches member names case-insensitively. Kebab-case values are derived from member names, so `read-only` matches `ReadOnly`.

## Parser behavior

Custom parsers may implement the sync or async parser interface for the target type. If a parser implements both, async parsing is preferred and `ANYCLI026` is reported. Async parsers are resolved from the configured service provider, or created with a public parameterless constructor when no service provider is configured. They receive the command's cancellation token and return a `ParameterParseResult<T>`.

`ISpanParsable<T>` extends `IParsable<T>`. A type implementing it must provide both string and span members. AnyCLI checks `ISpanParsable<T>` before `IParsable<T>`.

BCL types such as `int`, `Guid`, `DateOnly`, `TimeSpan`, and `decimal` already implement these interfaces and need no custom parser.

## Parser configuration

Set `Parser` on one argument or option to select a parser for that parameter. Register a parser assembly-wide with `ParameterValueParserAttribute<TValue, TParser>` for a type, or `EnumParameterParserAttribute<TParser>` for all enum parameters.

`ParameterValueParserAttribute` may be applied more than once for different types. Registering more than one parser for the same type reports `ANYCLI004`; the last registration overrides the previous one. Both registration attributes accept sync and async parsers.

Parser-related diagnostics such as `ANYCLI001`, `ANYCLI002`, `ANYCLI004`, and [`ANYCLI026`](diagnostics.md) are listed in the [diagnostics reference](diagnostics.md).

## Collections and nullable values

For a collection parameter, the element type is resolved through the same hierarchy. Nullable value types are unwrapped before resolution.

## API reference

- [`IParameterParser<T>`](xref:AnyCli.Parameters.Parsing.IParameterParser%601), [`IAsyncParameterParser<T>`](xref:AnyCli.Parameters.Parsing.IAsyncParameterParser%601)
- [`IEnumParameterParser`](xref:AnyCli.Parameters.Parsing.IEnumParameterParser), [`IAsyncEnumParameterParser`](xref:AnyCli.Parameters.Parsing.IAsyncEnumParameterParser)
- [`ParameterValueParserAttribute<TValue, TParser>`](xref:AnyCli.Parameters.Parsing.ParameterValueParserAttribute%602), [`EnumParameterParserAttribute<TParser>`](xref:AnyCli.Parameters.Parsing.EnumParameterParserAttribute%601)
- [`ParameterParseResult<T>`](xref:AnyCli.Parameters.Parsing.ParameterParseResult%601)

## Related

- [Diagnostics reference](diagnostics.md)
- [Parse custom types](../guides/custom-value-parsers.md)
- [Parameter attributes](parameter-attributes.md)
