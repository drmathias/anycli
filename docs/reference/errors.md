<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Errors reference

This page describes how AnyCLI represents, renders, and exits after command-line errors. For exact types, members, and signatures, use the API Reference in the top navigation.

## Error model

Command-line parsing failures are represented by a structured `CommandError` and thrown as a `CommandErrorException`.

Structured errors cover:

- command selection
- option and argument syntax
- required values
- invalid values

`ParameterSource` identifies whether an invalid value came from an argument, an option, or a global option.

## Error context

Errors are handled through `CommandErrorContext`, which contains:

- the application metadata
- the command path associated with the error
- the structured error
- the console error writer

Error output is written to standard error through an `IConsoleErrorWriter`.

## Handler pipeline

`ICommandErrorHandler` handles the error context. The default handler uses two separate components:

```text
CommandError
    -> ICommandErrorMessageProvider -> plain message
    -> ICommandErrorWriter          -> standard error
```

`ICommandErrorMessageProvider` turns the structured error into user-facing text. The default provider is localized.

`ICommandErrorWriter` writes that text. The default writer writes unstyled text.

`ConfigureErrorHandling` configures the handler, message provider, writer, and exit code. Type-based overloads are resolved from the application service provider when registered; otherwise, the public parameterless constructor is used. Instance overloads use the supplied object directly.

A custom handler can be created with `UseHandler(Func<CommandErrorHandlerContext, ICommandErrorHandler>)`. `CommandErrorHandlerContext` supplies the service provider, the configured message provider, the configured writer, the localization options, and a `DefaultHandler` that uses those configured components.

## Exit codes

Command-line errors return exit code `2` by default.

`UseExitCode` can set a fixed integer, a typed exit-code enum value, a function that returns an integer, or a function that returns a typed exit-code enum value from `CommandError`.

Instance overloads for `UseMessageProvider` and `UseWriter` reuse the supplied object across runs. Use the type-based overloads when the component should be resolved from DI or constructed per use.

## Scope

This pipeline handles command-line input errors. Exceptions thrown by command bodies are not converted into `CommandError` values.

Cancellation is separate. Cancellation propagates through `RunAsync` and is not represented as a `CommandError`.

## API reference

- [`CommandError`](xref:AnyCli.Errors.CommandError), [`CommandErrorException`](xref:AnyCli.Errors.CommandErrorException), [`CommandErrorContext`](xref:AnyCli.Errors.CommandErrorContext), [`CommandErrorHandlerContext`](xref:AnyCli.Errors.CommandErrorHandlerContext)
- [`ICommandErrorHandler`](xref:AnyCli.Errors.ICommandErrorHandler), [`DefaultCommandErrorHandler`](xref:AnyCli.Errors.DefaultCommandErrorHandler)
- [`ICommandErrorMessageProvider`](xref:AnyCli.Errors.ICommandErrorMessageProvider), [`LocalizedCommandErrorMessageProvider`](xref:AnyCli.Errors.LocalizedCommandErrorMessageProvider)
- [`ICommandErrorWriter`](xref:AnyCli.Errors.ICommandErrorWriter), [`PlainCommandErrorWriter`](xref:AnyCli.Errors.PlainCommandErrorWriter)
- [`ParameterSource`](xref:AnyCli.Errors.ParameterSource), [`CommandErrorResourceKeys`](xref:AnyCli.Errors.CommandErrorResourceKeys)
- [`CommandErrorHandlingBuilder`](xref:AnyCli.Commands.Registration.CommandErrorHandlingBuilder)
- [`IConsoleErrorWriter`](xref:AnyCli.Console.IConsoleErrorWriter), [`IOutputWriter`](xref:AnyCli.Console.IOutputWriter)

## Related

- [Customize command-line errors](../guides/customize-command-line-errors.md)
- [Help reference](help.md)
- [Understanding help and error handling](../explanation/understanding-help-and-errors.md)
