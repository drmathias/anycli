<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI015: Global option declared as argument

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI015` |
| Title | Global option declared as argument |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

A property on a `[GlobalOptions]` type uses `[Argument]`.

## Resolution

Global options must use `[Option]` or `[RequiredOption]`.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Global options](../../getting-started/global-options.md)
