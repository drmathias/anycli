<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Interceptors reference

Interceptors wrap command execution after command-line parsing and required-value validation succeed, before the command result is returned.

## Interfaces

| Interface | Delegate |
| --- | --- |
| `ICommandInterceptor` | `Func<CancellationToken, Task<int>>` |
| `ICommandInterceptor<TGlobalOptions>` | `Func<CancellationToken, Task<int>>` |
| `ICommandInterceptorWithExitCode<TExitCode>` | `Func<CancellationToken, Task<TExitCode>>` |
| `ICommandInterceptorWithExitCode<TGlobalOptions, TExitCode>` | `Func<CancellationToken, Task<TExitCode>>` |

`ICommandInterceptor<TGlobalOptions>` receives `CommandInterceptorContext<TGlobalOptions>` and requires the application to configure global options. `ICommandInterceptorWithExitCode<TExitCode>` converts the enum result to the integer process exit code through the framework bridge. `ICommandInterceptorWithExitCode<TGlobalOptions, TExitCode>` combines both: it receives typed global options and a typed exit-code delegate.

## Interceptor context

`CommandInterceptorContext` inherits from `CommandContext` and adds:

| Property | Type | Meaning |
| --- | --- | --- |
| `RawCommand` | `string` | Display string for the command line, including the application name. |
| `ParsedArguments` | `IReadOnlyDictionary<string, ParsedValue>` | Raw positional values keyed by argument name. |
| `ParsedOptions` | `IReadOnlyDictionary<string, ParsedValue>` | Raw option values keyed by primary option name. |
| `Name` | `string` | The command name. |
| `CommandPath` | `IReadOnlyList<string>` | Full command path. |
| `Console` | `IConsole` | Standard input, output, and error. |

`CommandInterceptorContext<TGlobalOptions>` adds:

| Property | Type |
| --- | --- |
| `GlobalOptions` | `TGlobalOptions` |

### Parsed value shapes

- `ParsedValue.Flag` represents a flag supplied without a value.
- `ParsedValue.Scalar` represents a single supplied value. `Value` is a string, or `null` when an optional argument was not supplied and has no default.
- `ParsedValue.Multiple` represents a variadic argument or option with multiple supplied values. `Values` is a read-only string list. A repeated option with one value appears as a scalar.
- Values remain strings even when the command parameter is an `int`, enum, or other parsed type.
- Option keys use the primary option name, such as `--priority`; aliases are not separate entries.
- `ParsedOptions` contains options supplied to the command. Options that were not supplied are absent, and typed global options are not included.

Use the command's typed parameters for normal command logic. The dictionaries are intended for cross-cutting interceptor concerns.

## Registration

Register an interceptor with `AddInterceptor<TInterceptor>()`:

```csharp
.AddInterceptor<TimingInterceptor>()
```

When a service provider is configured, interceptors are resolved from it. Otherwise they are created with a public parameterless constructor.

The AnyCli analyzer reports [`ANYCLI018`](diagnostics.md) when a global-options interceptor does not match the configured global options type, and [`ANYCLI019`](diagnostics.md) when an exit-code interceptor does not match the configured command exit code type.

For a full builder example, see [Write an interceptor](../guides/write-an-interceptor.md#register-the-interceptor).

## Execution order

Interceptors run in registration order before the command and in reverse order after it:

```text
first before
  second before
    command
  second after
first after
```

The first registered interceptor is the outermost wrapper.

## Scope

Interceptors run only when a command is selected and command-line parsing succeeds. They do not run for:

- help or version requests
- unknown commands or options
- missing or invalid parameters
- other command-line parsing errors

An interceptor can short-circuit by returning without calling `next`. Exceptions thrown by an interceptor or command propagate normally.

## Cancellation

The cancellation token passed to `InterceptAsync` is forwarded to `next`. An interceptor may pass a linked token to add a timeout or other cancellation condition. See [Handle Cancellation](../guides/handle-cancellation.md).

## Related

- [Write an interceptor](../guides/write-an-interceptor.md)
- [Diagnostics reference](diagnostics.md)
- [Logging With Interceptors](../getting-started/logging-with-interceptors.md)
- [Handle Cancellation](../guides/handle-cancellation.md)
- [Test a CLI application](../guides/test-a-cli-application.md)
