<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Set default values

Use attribute defaults or property initializers to provide values when an argument or option is omitted.

## Options

```csharp
public sealed partial record ListParameters
{
    [Option("--status", DefaultValue = "open")]
    public string Status { get; set; } = "";
}
```

```csharp
[Option("--count", DefaultValue = 10)]
public int Count { get; set; }
```

## Optional arguments

```csharp
public sealed partial record CopyParameters
{
    [Argument(0)]
    public required string Source { get; init; }

    [OptionalArgument(1, DefaultValue = "output.txt")]
    public string Destination { get; set; } = "";
}
```

## Variadic arguments

```csharp
[VariadicArgument(1, DefaultValue = new[] { "all" })]
public string[] Values { get; set; } = [];
```

## Property initializers

A property initializer is used when no command-line value or attribute default is supplied:

```csharp
[Option("--data")]
public string DataFile { get; set; } = "tasks.json";
```

## Priority

When a parameter is omitted, the order is:

1. Attribute `DefaultValue`.
2. Property initializer.

An explicitly supplied command-line value always overrides both.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Define optional and variadic arguments](optional-and-variadic-arguments.md)
