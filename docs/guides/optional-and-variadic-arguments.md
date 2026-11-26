<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Define optional and variadic arguments

Use `[OptionalArgument]` when a positional value can be omitted and `[VariadicArgument]` when a command accepts multiple positional values.

## Optional arguments

Declare an optional positional argument with its position:

```csharp
public sealed partial record CopyParameters
{
    [Argument(0)]
    public required string Source { get; init; }

    [OptionalArgument(1)]
    public string? Destination { get; init; }
}
```

Both forms are valid:

```bash
copy input.txt output.txt
copy input.txt
```

Without a configured default, an omitted optional argument is `null` for nullable types.

## Variadic arguments

Declare a variadic argument with `[VariadicArgument]`:

```csharp
public sealed partial record TagParameters
{
    [VariadicArgument(0, MinimumCount = 1)]
    public required string[] Tags { get; init; }
}
```

`MinimumCount = 1` requires at least one value when values are supplied. If the argument has a property initializer or default value provider, omitting all values uses that default without applying the minimum or maximum, and help output treats the argument as optional. Use `MaximumCount` to set an upper bound for user-supplied values:

```csharp
[VariadicArgument(0, MinimumCount = 1, MaximumCount = 3)]
public required string[] Tags { get; init; }
```

A variadic argument accepts zero values by default:

```csharp
[VariadicArgument(0)]
public string[] Values { get; set; } = [];
```

## Errors

Supplying too few or too many variadic values, or omitting a required positional argument, produces a structured command-line error and returns exit code `2` by default. If a variadic argument has a default source, supplying zero values uses the default instead.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Set default values](default-values.md)
