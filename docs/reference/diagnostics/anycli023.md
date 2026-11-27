<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI023: Default value display without default value

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI023` |
| Title | Default value display without default value |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Warning |
| Code fix available | No |

## Cause

A parameter has default display metadata but no default provider or property initializer.

## Resolution

The display metadata is ignored. Add a default source or remove the metadata.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Set default values](../../guides/default-values.md)
- [Parameter attributes](../parameter-attributes.md)
