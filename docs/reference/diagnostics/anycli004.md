<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI004: Duplicate parameter parser registration

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI004` |
| Title | Duplicate parameter parser registration |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Warning |
| Code fix available | No |

## Cause

More than one parser is registered for the same type.

## Resolution

The last registration overrides the previous registration. Remove duplicate registrations to silence the warning.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter parsing reference](../parameter-parsing.md)
- [Parse custom types](../../guides/custom-value-parsers.md)
