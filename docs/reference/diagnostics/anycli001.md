<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI001: Unsupported parameter type

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI001` |
| Title | Unsupported parameter type |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

The parameter property type is not `string`, an enum, `ISpanParsable<T>`, or `IParsable<T>`, and no parser is configured.

## Resolution

Use a supported type, implement `IParsable<T>` or `ISpanParsable<T>`, set `Parser` on the attribute, or register a parser with `ParameterValueParserAttribute<TValue, TParser>`.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter parsing reference](../parameter-parsing.md)
- [Parse custom types](../../guides/custom-value-parsers.md)
