<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Interceptors reference

Interceptors wrap command execution after command-line parsing and required-value validation succeed, before the command result is returned. For exact interfaces and members, use the API Reference in the top navigation.

## Interceptor kinds

`ICommandInterceptor` is the non-generic base. `ICommandInterceptor<TGlobalOptions>` receives typed global options. `ICommandInterceptorWithExitCode<TExitCode>` works with a typed exit-code result, and `ICommandInterceptorWithExitCode<TGlobalOptions, TExitCode>` combines typed global options and a typed exit code.

The generic global-options variants require the application to configure global options. The exit-code variants convert the enum result to the integer process exit code through the framework bridge.

## Interceptor context

`CommandInterceptorContext` inherits from `CommandContext` and adds the raw command display string plus raw parsed arguments and options. `CommandInterceptorContext<TGlobalOptions>` also exposes typed global options.

The parsed dictionaries are intended for cross-cutting interceptor concerns. Use the command's typed parameters for normal command logic.

### Parsed value shapes

- `ParsedValue.Flag` represents a flag supplied without a value.
- `ParsedValue.Scalar` represents a single supplied value. `Value` is a string, or `null` when an optional argument was not supplied and has no default.
- `ParsedValue.Multiple` represents a variadic argument or option with multiple supplied values. A repeated option with one value appears as a scalar.
- Values remain strings even when the command parameter is an `int`, enum, or other parsed type.
- Option keys use the primary option name, such as `--priority`; aliases are not separate entries.
- `ParsedOptions` contains options supplied to the command. Options that were not supplied are absent, and typed global options are not included.

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

## API reference

- [`ICommandInterceptor`](xref:AnyCli.Interceptors.ICommandInterceptor)
- [`ICommandInterceptor<TGlobalOptions>`](xref:AnyCli.Interceptors.ICommandInterceptor%601)
- [`ICommandInterceptorWithExitCode<TExitCode>`](xref:AnyCli.Interceptors.ICommandInterceptorWithExitCode%601)
- [`ICommandInterceptorWithExitCode<TGlobalOptions, TExitCode>`](xref:AnyCli.Interceptors.ICommandInterceptorWithExitCode%602)
- [`CommandInterceptorContext`](xref:AnyCli.Interceptors.CommandInterceptorContext)
- [`CommandInterceptorContext<TGlobalOptions>`](xref:AnyCli.Interceptors.CommandInterceptorContext%601)
- [`ParsedValue`](xref:AnyCli.Interceptors.ParsedValue)

## Related

- [Write an interceptor](../guides/write-an-interceptor.md)
- [Diagnostics reference](diagnostics.md)
- [Logging With Interceptors](../getting-started/logging-with-interceptors.md)
- [Handle Cancellation](../guides/handle-cancellation.md)
- [Test a CLI application](../guides/test-a-cli-application.md)
