<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI017: Invalid variadic maximum count

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI017` |
| Title | Invalid variadic maximum count |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

`MaximumCount` is not greater than `1`, or is less than `MinimumCount`.

## Resolution

Set `MaximumCount` greater than one and at least `MinimumCount`, or use a scalar parameter.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Optional and variadic arguments](../../guides/optional-and-variadic-arguments.md)
