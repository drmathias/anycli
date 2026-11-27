<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI007: Invalid argument position

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI007` |
| Title | Invalid argument position |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

Argument positions are not unique, non-negative, and contiguous starting at `0`.

## Resolution

Correct the `Argument` or `OptionalArgument` positions so they are unique and start at zero.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Optional and variadic arguments](../../guides/optional-and-variadic-arguments.md)
