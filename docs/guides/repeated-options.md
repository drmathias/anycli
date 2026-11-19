<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Accept repeated options

Use a collection-typed property to accept an option multiple times.

```csharp
public sealed partial record AddParameters
{
    [Option("--tag", "-t")]
    public string[] Tags { get; set; } = [];
}
```

```bash
add "Write docs" --tag docs --tag release
add "Write docs" -t docs -t release
```

The resulting collection contains the values in command-line order.

## Collection types

Common collection targets are supported, including arrays, lists, sets, and immutable collections:

```csharp
[Option("--include")]
public List<string> Include { get; set; } = [];

[Option("--label")]
public HashSet<string> Labels { get; set; } = [];
```

Set-shaped targets remove duplicate values. Other collection targets preserve all values.

## Scalar options

A scalar option cannot be repeated:

```csharp
[Option("--name")]
public string Name { get; set; } = "";
```

Supplying `--name` more than once produces a structured command-line error.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Command-line syntax](../reference/command-line-syntax.md)
