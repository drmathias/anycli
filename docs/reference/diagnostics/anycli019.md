<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# ANYCLI019: Interceptor exit code mismatch

| Property | Value |
| --- | --- |
| Rule ID | `ANYCLI019` |
| Title | Interceptor exit code mismatch |
| Category | `AnyCli` |
| Diagnostic type | Analyzer |
| Enabled by default | Yes |
| Default severity | Error |
| Code fix available | No |

## Cause

An interceptor requires a command exit code type that the application does not configure.

## Resolution

Call `ConfigureCommands<TExitCode>(...)` with the required exit code type on the application builder.

## Related

- [Diagnostics overview](../diagnostics.md)
- [Interceptors reference](../interceptors.md)
- [Write an interceptor](../../guides/write-an-interceptor.md)
