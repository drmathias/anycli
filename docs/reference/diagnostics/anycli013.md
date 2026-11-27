<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI013: Invalid option alias

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI013` |
| Title | Invalid option alias |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

An option alias is not a valid short name such as `-o` or long name such as `--output`.

## Resolution

Correct the alias.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Command-line syntax](../command-line-syntax.md)
