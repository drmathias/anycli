<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI005: Invalid variadic minimum count

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI005` |
| Title | Invalid variadic minimum count |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

`MinimumCount` is negative.

## Resolution

Set `MinimumCount` to zero or a positive value.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Optional and variadic arguments](../../guides/optional-and-variadic-arguments.md)
