<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Help, errors, and output

AnyCLI separates command output, help rendering, and command-line error handling. Keeping those concerns separate lets you style one without changing the others.

## Output channels

Commands receive a `CommandContext` with a `Console` property. The console wraps the standard streams:

- `Console.Output` writes to standard output.
- `Console.Error` writes to standard error.
- `Console.Input` reads from standard input.

The writer interfaces are intentionally small. `IOutputWriter` exposes only `Write` and `WriteLine`; disposal and flushing belong to the application host. This makes command output testable and prevents commands from owning process-level streams.

Help is written to standard output. Command-line errors are written to standard error. Command results can use either stream depending on whether they are successful output or failures.

## Help pipeline

Help is built from command metadata and then rendered by an `IHelpProvider`:

```text
commands, options, arguments
        -> help provider
        -> HelpContext.Console.Output
```

The help provider decides the layout and writes the result. The default provider is `DefaultHelpProvider`.

Text is a separate concern. `IHelpTextProvider` supplies section headings, usage placeholders, and descriptions. The default provider is `LocalizedHelpTextProvider`, so help text can be localized without replacing the renderer.

A custom help provider factory receives a `HelpProviderContext` containing:

- the application service provider, when configured
- the configured `IHelpTextProvider`
- the configured `IHelpValueFormatter`
- the configured localization options

Root help and command help receive different contexts. `RootHelpContext` contains the top-level commands and global options. `CommandHelpContext` contains the command or command group being described.

## Error pipeline

Invalid command-line input is represented by a structured `CommandError` and wrapped in a `CommandErrorException`. `ConsoleApp.RunAsync` catches that exception and passes a `CommandErrorContext` to an `ICommandErrorHandler`.

The default handler divides the work into two parts:

```text
CommandError
    -> ICommandErrorMessageProvider -> plain message
    -> ICommandErrorWriter          -> standard error
```

The message provider turns the structured error into user-facing text. The default provider is localized. The writer renders that plain message. The default writer writes plain text without styling.

The error context also contains the app name, command path, and error console. A handler can use those details to produce a different response, while a writer can change only the presentation.

The exit code is configured separately. Command-line errors return `2` by default. Use `UseExitCode` to return a different value or derive one from the error context.

## What this pipeline does not cover

This pipeline handles command-line input errors such as unknown options, missing arguments, and invalid values. Exceptions thrown by command bodies are not command-line errors and are not converted by this pipeline.

Cancellation is also separate. `RunAsync` passes its cancellation token through command execution; cancellation is not represented as a `CommandError`.

## Why the separation matters

Each layer can change independently:

- Replace the help renderer without changing help text or localization.
- Replace the error writer without changing messages or exit codes.
- Replace the message provider without changing layout.
- Add a rich console implementation without changing commands.

The Spectre.Console adapter uses exactly this structure. It provides a help provider, an error writer, and ANSI consoles, while leaving the underlying help text, error messages, and exit-code behavior in place.

## Related

- [Help and error pipeline reference](../reference/help-and-error-pipeline.md)
- [The Spectre.Console adapter](spectre-adapter.md)
