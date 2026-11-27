<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI018: Interceptor global options mismatch

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI018` |
| Title | Interceptor global options mismatch |
| Category | `AnyCli` |
| Diagnostic type | Analyzer |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

An interceptor requires typed global options that the application does not configure.

## Resolution

Call `AddGlobalOptions<TGlobalOptions>()` with the required global options type on the application builder.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Interceptors reference](../interceptors.md)
- [Write an interceptor](../../guides/write-an-interceptor.md)
