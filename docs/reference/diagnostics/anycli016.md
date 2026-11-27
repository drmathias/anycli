<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI016: Conflicting parameters kind

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI016` |
| Title | Conflicting parameters kind |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

A type is marked `[GlobalOptions]` and also implements `ICommandParameters<T>`.

## Resolution

Use one role. Command parameter types should inherit from the global options type instead.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Global options](../../getting-started/global-options.md)
