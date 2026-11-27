<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI002: Invalid parameter parser

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI002` |
| Title | Invalid parameter parser |
| Category | `AnyCli` |
| Diagnostic type | Source generator |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

The configured `Parser` type does not implement `IParameterParser<T>` or `IAsyncParameterParser<T>` for the property type, or the parser type is not valid for async instantiation.

## Resolution

Make the parser implement the matching closed parser interface. Async parsers must be instantiable classes and are resolved from DI or a public parameterless constructor.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Parameter parsing reference](../parameter-parsing.md)
- [Parse custom types](../../guides/custom-value-parsers.md)
