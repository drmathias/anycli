<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI023: Help default value without default value

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI023` |
| Title | Help default value without default value |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Warning |
| Code fix available | No |

## Cause

A parameter has `HelpDefaultValue` metadata but no default provider or property initializer.

## Resolution

The help default value metadata is ignored. Add a default source or remove the metadata.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Set default values](../../guides/default-values.md)
- [Parameter attributes](../parameter-attributes.md)
