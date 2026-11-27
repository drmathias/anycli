<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI025: Ambiguous default value provider

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI025` |
| Title | Ambiguous default value provider |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Warning |
| Code fix available | No |

## Cause

A provider type implements both `IDefaultValueProvider<T>` and `IAsyncDefaultValueProvider<T>`.

## Resolution

The async provider is used. Remove one implementation to silence the warning.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Set default values](../../guides/default-values.md)
- [Parameter attributes](../parameter-attributes.md)
