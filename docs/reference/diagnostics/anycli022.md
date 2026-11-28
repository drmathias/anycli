<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI022: Help default value on required parameter

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI022` |
| Title | Help default value on required parameter |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Warning |
| Code fix available | No |

## Cause

A required parameter has `HelpDefaultValue` metadata.

## Resolution

The help default value metadata is ignored. Remove it or make the parameter optional.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Set default values](../../guides/default-values.md)
- [Parameter attributes](../parameter-attributes.md)
