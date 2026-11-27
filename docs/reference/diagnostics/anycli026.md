<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI026: Ambiguous parameter parser

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI026` |
| Title | Ambiguous parameter parser |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Warning |
| Code fix available | No |

## Cause

A parser type implements both the sync and async parser interfaces for the parameter type.

## Resolution

Async parsing is preferred. Remove one implementation to silence the warning.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter parsing reference](../parameter-parsing.md)
- [Parse custom types](../../guides/custom-value-parsers.md)
