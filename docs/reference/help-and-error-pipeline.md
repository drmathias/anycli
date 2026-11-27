<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Help and error pipeline reference

This page describes how AnyCLI selects and invokes console, help, and command-line error components. For exact types, members, and signatures, use the API Reference in the top navigation.

## Console abstractions

Commands receive an `IConsole`, which exposes standard input, output, and error. Help and command-line errors receive narrower writer interfaces: help writes through an `IConsoleWriter`, and errors write through an `IConsoleErrorWriter`. Both expose an `IOutputWriter` for the relevant stream.

The narrow interfaces keep help rendering and error formatting from owning process-level streams.

## Help pipeline

Help is built from command metadata and rendered by a help provider:

```text
command, option, and argument metadata
    -> help provider
    -> HelpContext.Console.Output
```

The provider decides the layout and writes the result. `DefaultHelpProvider` is the built-in plain-text provider. Text is a separate concern: `IHelpTextProvider` supplies headings, labels, placeholders, and descriptions, while `IHelpValueFormatter` formats fallback default values such as enums and collections. The defaults are `LocalizedHelpTextProvider` and `DefaultHelpValueFormatter`.

`ConfigureHelp` configures shared help services and selects the provider. Shared text and value formatting apply to any provider factory through `HelpProviderContext`. When no provider is selected, `DefaultHelpProvider` is used. A help provider registered in the configured service provider is resolved from it; otherwise, a public parameterless constructor is used.

Root and command help use different contexts. Both expose shared `Application` metadata and a `Console`. `CommandHelpContext` adds the selected command or command group. `Application` contains the application name, global options, and root command tree.

Default value display metadata affects help output only. It does not change binding or invoke a default value provider. Built-in resource keys for help text are exposed by `HelpTextResourceKeys`.

## Error pipeline

Command-line parsing failures are represented by a structured `CommandError` and thrown as a `CommandErrorException`. `ConsoleApp.RunAsync` catches that exception and delegates it to an `ICommandErrorHandler`.

The default handler divides the work into two parts:

```text
CommandError
    -> ICommandErrorMessageProvider -> plain message
    -> ICommandErrorWriter          -> standard error
```

The message provider turns the structured error into user-facing text. The default provider is localized. The writer renders that plain message. The default writer writes unstyled text.

Structured errors cover command selection, option and argument syntax, required values, and invalid values. `ParameterSource` identifies whether an invalid value came from an argument, option, or global option.

`ConfigureErrorHandling` configures the handler, message provider, writer, and exit code. The default command-line error exit code is `2`. Built-in resource keys for error messages are exposed by `CommandErrorResourceKeys`.

## Scope

The error pipeline handles command-line input errors. Exceptions thrown by command bodies are not converted into `CommandError` values. Cancellation is separate and propagates from `RunAsync`.

## API reference

- Console: [`IConsole`](xref:AnyCli.Console.IConsole), [`IConsoleWriter`](xref:AnyCli.Console.IConsoleWriter), [`IConsoleErrorWriter`](xref:AnyCli.Console.IConsoleErrorWriter), [`IOutputWriter`](xref:AnyCli.Console.IOutputWriter)
- Help: [`IHelpProvider`](xref:AnyCli.Help.IHelpProvider), [`DefaultHelpProvider`](xref:AnyCli.Help.DefaultHelpProvider), [`IHelpTextProvider`](xref:AnyCli.Help.IHelpTextProvider), [`IHelpValueFormatter`](xref:AnyCli.Help.IHelpValueFormatter), [`HelpProviderContext`](xref:AnyCli.Help.HelpProviderContext), [`ApplicationInfo`](xref:AnyCli.Commands.Metadata.ApplicationInfo), [`RootHelpContext`](xref:AnyCli.Help.RootHelpContext), [`CommandHelpContext`](xref:AnyCli.Help.CommandHelpContext), [`HelpTextResourceKeys`](xref:AnyCli.Help.HelpTextResourceKeys), [`HelpConfigurationBuilder`](xref:AnyCli.Commands.Registration.HelpConfigurationBuilder)
- Errors: [`CommandError`](xref:AnyCli.Errors.CommandError), [`CommandErrorException`](xref:AnyCli.Errors.CommandErrorException), [`ICommandErrorHandler`](xref:AnyCli.Errors.ICommandErrorHandler), [`ICommandErrorMessageProvider`](xref:AnyCli.Errors.ICommandErrorMessageProvider), [`ICommandErrorWriter`](xref:AnyCli.Errors.ICommandErrorWriter), [`CommandErrorResourceKeys`](xref:AnyCli.Errors.CommandErrorResourceKeys), [`ParameterSource`](xref:AnyCli.Errors.ParameterSource), [`CommandErrorHandlingBuilder`](xref:AnyCli.Commands.Registration.CommandErrorHandlingBuilder)

## Related

- [Help, errors, and output](../explanation/help-errors-and-output.md)
- [Spectre.Console adapter reference](spectre-console-adapter.md)
