<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI020: Invalid default value provider

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI020` |
| Title | Invalid default value provider |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

A `DefaultValueProvider` type is not a non-abstract class implementing `IDefaultValueProvider<T>` or `IAsyncDefaultValueProvider<T>` for the property type.

## Resolution

Use a compatible provider type.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Set default values](../../guides/default-values.md)
- [Parameter attributes](../parameter-attributes.md)
