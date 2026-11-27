<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Parameter attributes

This page describes how parameter attributes are declared and how their values affect parsing and help. For exact members and signatures, use the API Reference in the top navigation.

## Attributes

| Attribute | Purpose | Arity |
| --- | --- | --- |
| `[Argument(position)]` | Required positional value | Exactly one |
| `[OptionalArgument(position)]` | Positional value that can be omitted | Zero or one |
| `[VariadicArgument(position)]` | Multiple positional values | Zero or more by default |
| `[Option(name, aliases...)]` | Optional named value or flag | Zero or one, or repeated for collections |
| `[RequiredOption(name, aliases...)]` | Named option that must be supplied | One |
| `[GlobalOptions]` | Marks a type as global options | Not applicable |

Parameter types must be declared `partial` and be either a `class` or class-based `record`.

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

`[VariadicArgument]` accepts zero or more positional values by default. Use `MinimumCount` and `MaximumCount` to constrain the number of user-supplied values. A property initializer or `DefaultValueProvider` is used without applying those bounds when no values are supplied, and the argument is treated as optional if a default value is configured.

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
public string Output { get; init; } = "";
```

When no name is supplied, AnyCLI generates a kebab-case long name from the property name:

```csharp
[Option]
public bool DryRun { get; init; }
```

```bash
run --dry-run
```

## Required options

`[RequiredOption]` fails parsing when the option is not supplied:

```csharp
[RequiredOption("--config", "-c")]
public required string Config { get; init; }
```

Required options do not support supplying default values; the option must be present on the command line.

## Defaults

Defaults come from a property initializer or a `DefaultValueProvider`. Explicit command-line values take priority.

1. Explicit command-line value.
2. `DefaultValueProvider`.
3. Property initializer.
4. `default(T)`.

Property initializers are preserved when no command-line value or provider default is supplied.

Simple initializers are captured by the generator and shown in help output. Computed initializers are not evaluated for help.

### Default value display

Use `[DefaultValueDisplay]` for literal help text and `[DefaultValueDisplayResource]` for a resource key:

```csharp
[Option("--data")]
[DefaultValueDisplay("tasks.json")]
[DefaultValueDisplayResource("DataFileDefault")]
public string DataFile { get; init; } = "tasks.json";
```

Display metadata affects help output only. It overrides captured initializer values and does not change binding behavior. It is ignored for required parameters and when no default source is configured.

## Supported types

AnyCLI supports:

- `string`
- Enums, matched case-insensitively and with canonical kebab-case names
- Types implementing `IParsable<T>` or `ISpanParsable<T>`
- Collections of supported scalar types
- Custom types with an `IParameterParser<T>` or `IAsyncParameterParser<T>` implementation

## Collections

Collection-typed arguments and options accept multiple values. Supported target types are:

- Single-dimensional arrays: `T[]`
- Read-only interfaces: `IReadOnlyList<T>`, `IReadOnlyCollection<T>`, `IReadOnlySet<T>`
- Mutable interfaces: `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `ISet<T>`
- Mutable collections: `List<T>`, `HashSet<T>`, `LinkedList<T>`, `Collection<T>`
- Read-only collection: `ReadOnlyCollection<T>`
- Frozen and immutable collections: `FrozenSet<T>`, `ImmutableArray<T>`, `ImmutableList<T>`, `ImmutableHashSet<T>`

```csharp
[Option("--tag", "-t")]
public string[] Tags { get; init; } = [];
```

```bash
add "Write docs" --tag docs --tag release
```

## Descriptions

Use `[Description]` for literal help text or `[ResourceDescription]` for a resource key. The description is shown in help output and can be localized through the configured text provider.

## API reference

- [`ArgumentAttribute`](xref:AnyCli.Parameters.Arguments.ArgumentAttribute), [`OptionalArgumentAttribute`](xref:AnyCli.Parameters.Arguments.OptionalArgumentAttribute), [`VariadicArgumentAttribute`](xref:AnyCli.Parameters.Arguments.VariadicArgumentAttribute)
- [`OptionAttribute`](xref:AnyCli.Parameters.Options.OptionAttribute), [`RequiredOptionAttribute`](xref:AnyCli.Parameters.Options.RequiredOptionAttribute), [`GlobalOptionsAttribute`](xref:AnyCli.Parameters.Options.GlobalOptionsAttribute)
- [`ArgumentAttributeBase`](xref:AnyCli.Parameters.Arguments.ArgumentAttributeBase), [`OptionAttributeBase`](xref:AnyCli.Parameters.Options.OptionAttributeBase)
- [`DefaultValueDisplayAttribute`](xref:AnyCli.Help.DefaultValueDisplayAttribute), [`DefaultValueDisplayResourceAttribute`](xref:AnyCli.Help.DefaultValueDisplayResourceAttribute)

## Related

- [Command-line syntax](command-line-syntax.md)
- [Define optional and variadic arguments](../guides/optional-and-variadic-arguments.md)
- [Require an option](../guides/required-options.md)
- [Accept repeated options](../guides/repeated-options.md)
- [Set default values](../guides/default-values.md)
- [Parse custom types](../guides/custom-value-parsers.md)
