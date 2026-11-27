<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Parameter parsing reference

AnyCLI resolves a parser for every argument and option value at compile time. It does not use `TypeConverter` or a convention-based `TryParse` method. This page lists the exact precedence, supported contracts, and diagnostics.

## Resolution order

| Priority | Rule | Notes |
| ---: | --- | --- |
| 1 | Explicit `Parser` on the argument or option | Highest precedence. |
| 2 | Registered `ParameterValueParserAttribute<TValue, TParser>` | One parser per value type. |
| 3 | `string` | Passed through without conversion. |
| 4 | Enum | Default enum parser or a registered `EnumParameterParserAttribute<TParser>`. |
| 5 | `ISpanParsable<T>` | Uses the span-based `TryParse`. |
| 6 | `IParsable<T>` | Uses the string-based `TryParse`. |
| 7 | Unsupported | Source generator emits [`ANYCLI001`](diagnostics.md). |

Explicit and registered parsers override enum handling for a specific enum type.

The default enum parser matches member names case-insensitively. Kebab-case values are derived from member names, so `read-only` matches `ReadOnly`.

## Parsing contracts

| Contract | Member used |
| --- | --- |
| `IParsable<T>` | `static bool TryParse(string? value, IFormatProvider? provider, out T result)` |
| `ISpanParsable<T>` | `static bool TryParse(ReadOnlySpan<char> value, IFormatProvider? provider, out T result)` |
| `IParameterParser<T>` | `static bool TryParse(string value, IFormatProvider? provider, out T result)` |
| `IAsyncParameterParser<T>` | `ValueTask<ParameterParseResult<T>> TryParseAsync(string value, IFormatProvider? provider, CancellationToken cancellationToken)` |
| `IEnumParameterParser` | `static bool TryParse<TEnum>(string value, IFormatProvider? provider, out TEnum result)` |
| `IAsyncEnumParameterParser` | `ValueTask<ParameterParseResult<TEnum>> TryParseAsync<TEnum>(string value, IFormatProvider? provider, CancellationToken cancellationToken)` |

`ISpanParsable<T>` extends `IParsable<T>`. A type implementing it must provide both string and span members. AnyCLI checks `ISpanParsable<T>` before `IParsable<T>`.

A parser type must implement either the sync or async parser interface for the target type, not both. Async parsers are resolved from the configured service provider, or created with a public parameterless constructor when no service provider is configured. They also receive the command's cancellation token.

BCL types such as `int`, `Guid`, `DateOnly`, `TimeSpan`, and `decimal` already implement these interfaces and need no custom parser.

## Parser configuration

| API | Scope |
| --- | --- |
| `Parser = typeof(TParser)` | One argument or option. |
| `ParameterValueParserAttribute<TValue, TParser>` | One value type across the assembly. |
| `EnumParameterParserAttribute<TParser>` | All enum parameters in the assembly. |

`ParameterValueParserAttribute` may be applied more than once for different value types. Registering more than one parser for the same value type produces `ANYCLI004`.

`EnumParameterParserAttribute` may be applied once. It supplies the application-wide enum parser. Both attributes accept sync and async parsers.

Parser-related diagnostics such as `ANYCLI001`, `ANYCLI002`, `ANYCLI004`, and [`ANYCLI026`](diagnostics.md) are listed in the [diagnostics reference](diagnostics.md).

## Collections and nullable values

For a collection parameter, the element type is resolved through the same hierarchy. Nullable value types are unwrapped before resolution.

## Related

- [Diagnostics reference](diagnostics.md)
- [Parse custom value types](../guides/custom-value-parsers.md)
- [Parameter attributes](parameter-attributes.md)
