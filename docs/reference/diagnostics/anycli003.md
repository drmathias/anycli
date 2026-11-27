<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI003: Invalid variadic parameter

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI003` |
| Title | Invalid variadic parameter |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

A variadic argument or repeated option uses an unsupported collection type.

## Resolution

Use an array or one of the supported collection types listed in the parameter attributes reference.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter attributes](../parameter-attributes.md)
- [Optional and variadic arguments](../../guides/optional-and-variadic-arguments.md)
