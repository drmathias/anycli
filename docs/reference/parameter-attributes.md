<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Parameter attributes

This page lists the attributes used to declare command parameters and the rules that apply to them.

## Attributes

| Attribute | Purpose | Arity |
| --- | --- | --- |
| `[Argument(position)]` | Required positional value | Exactly one |
| `[OptionalArgument(position)]` | Positional value that can be omitted | Zero or one |
| `[VariadicArgument(position)]` | Multiple positional values | Zero or more by default |
| `[Option(name, aliases...)]` | Optional named value or flag | Zero or one, or repeated for collections |
| `[RequiredOption(name, aliases...)]` | Named option that must be supplied | One |
| `[GlobalOptions]` | Marks a type as global options | Not applicable |

Parameter types must be declared `partial`. They can be classes or records, and properties can use `get; set;` or `get; init;`. The `required` modifier is supported.

## Argument positions

Positions are zero-based and correspond to the order of positional values on the command line.

```csharp
public sealed partial record CopyParameters
{
    [Argument(0)]
    public required string Source { get; init; }

    [OptionalArgument(1)]
    public string? Destination { get; init; }
}
```

```bash
copy input.txt output.txt
copy input.txt
```

## Variadic arguments

`[VariadicArgument]` accepts zero or more positional values by default. Use `MinimumCount` and `MaximumCount` to constrain the number of values.

```csharp
public sealed partial record TagParameters
{
    [VariadicArgument(0, MinimumCount = 1, MaximumCount = 3)]
    public required string[] Tags { get; init; }
}
```

```bash
tag docs
tag docs release
```

The default values are `MinimumCount = 0` and `MaximumCount = int.MaxValue`.

## Option names

Options can use explicit names and aliases:

```csharp
[Option("--output", "-o")]
public string Output { get; set; } = "";
```

When no name is supplied, AnyCLI generates a kebab-case long name from the property name:

```csharp
[Option]
public bool DryRun { get; set; }
```

```bash
run --dry-run
```

## Required options

`[RequiredOption]` fails parsing when the option is not supplied:

```csharp
[RequiredOption("--config", "-c")]
public string Config { get; set; } = "";
```

Required options do not use `DefaultValue`; the option must be present on the command line.

## Attribute properties

| Property | Applies to | Meaning |
| --- | --- | --- |
| `Position` | Argument attributes | Zero-based positional index |
| `Name` | Argument and option attributes | Explicit argument or primary option name |
| `Aliases` | Option attributes | Additional option names |
| `DefaultValue` | Optional/variadic arguments and options | Value used when the parameter is omitted |
| `MinimumCount` | `[VariadicArgument]` | Minimum number of values |
| `MaximumCount` | `[VariadicArgument]` | Maximum number of values |
| `Parser` | Argument and option attributes | Custom `IParameterParser<T>` implementation |

## Defaults

Defaults can come from the attribute or from the property initializer. Explicit command-line values take priority.

1. Explicit command-line value.
2. Attribute `DefaultValue`.
3. Property initializer.

Property initializers are preserved when no command-line value or attribute default is supplied.

## Supported value types

AnyCLI supports:

- `string`
- Enums, matched case-insensitively and with kebab-case names
- Types implementing `IParsable<T>` or `ISpanParsable<T>`
- Collections of supported scalar types
- Custom types with an `IParameterParser<T>` implementation

## Collections

Collection-typed arguments and options accept multiple values. Supported targets include common arrays, lists, sets, and immutable collections. A scalar option supplied more than once produces a structured error.

```csharp
[Option("--tag", "-t")]
public string[] Tags { get; set; } = [];
```

```bash
add "Write docs" --tag docs --tag release
```

## Descriptions

Use `[Description]` for literal help text or `[ResourceDescription]` for a resource key. The description is shown in help output and can be localized through the configured text provider.

## Related

- [Command-line syntax](command-line-syntax.md)
- [Define optional and variadic arguments](../guides/optional-and-variadic-arguments.md)
- [Require an option](../guides/required-options.md)
- [Accept repeated options](../guides/repeated-options.md)
- [Set default values](../guides/default-values.md)
- [Parse custom value types](../guides/custom-value-parsers.md)
