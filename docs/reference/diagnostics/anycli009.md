<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI009: Invalid option name

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI009` |
| Title | Invalid option name |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

An option name is not a valid short name such as `-o` or long name such as `--output`.

## Resolution

Correct the option name.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Command-line syntax](../command-line-syntax.md)
