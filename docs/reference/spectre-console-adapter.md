<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Spectre.Console adapter reference

The `AnyCli.SpectreConsole` package connects AnyCLI contexts to Spectre.Console. Install it with:

```bash
dotnet add package AnyCli.SpectreConsole
```

Use the `AnyCli.SpectreConsole` namespace for the adapter, providers, writers, and options. For exact members and signatures, use the API Reference in the top navigation.

## What the adapter adds

The adapter connects AnyCLI's console abstractions to Spectre.Console:

- `context.AnsiConsole.Output` and `context.AnsiConsole.Error` on command contexts
- `context.AnsiConsole.Output` on help contexts
- `context.AnsiConsole.Error` on command error contexts
- `UseSpectreHelp` for styled help through `SpectreHelpProvider`
- `UseSpectreErrors` for styled errors through `SpectreCommandErrorWriter`

The ANSI consoles are created from the existing output writers and cached per context. Raw writes through `context.Console.Output` remain available and can be mixed with Spectre output.

## What it does not replace

The adapter is a presentation layer. It does not replace:

- the help text provider that supplies headings, descriptions, and localization
- the help value formatter that displays fallback defaults
- the error message provider that turns structured errors into text
- the error exit-code configuration
- command registration or parameter binding

`UseSpectreHelp` uses the shared `IHelpTextProvider` and `IHelpValueFormatter` configured on `ConfigureHelp`, so headings, descriptions, and default values work the same way as the default provider. `UseSpectreErrors` replaces only the writer; the message provider and exit code remain unchanged.

## Help behavior

`SpectreHelpOptions` controls only Spectre-specific styles. Unless overridden, help uses aqua headings, grey option placeholders, white command placeholders, and medium-purple argument syntax. Usage lines, commands, options, and descriptions use plain styles by default.

Help rendering only uses the output stream. The underlying help context also exposes an error console because it shares the console-writer abstraction, but that stream is not part of the help pipeline.

## Error behavior

`SpectreCommandErrorWriter` renders the plain error message with Spectre styling. `SpectreErrorOptions` controls the error style and optional console options. The default error style is red, and a `null` console options value uses the error context console.

## Console options

`SpectreConsoleOptions` controls ANSI output, color-system detection, and interaction detection. By default, ANSI and color support are detected and interaction is disabled. `SpectreConsoleOptions.ForTesting` forces deterministic ANSI output with the standard color system.

## Testing

Use `SpectreConsoleOptions.ForTesting` when asserting rendered output. The adapter also supports direct console creation through `SpectreConsoleAdapter.Create` for services that do not receive a command context.

## API reference

- [`SpectreConsoleAdapter`](xref:AnyCli.SpectreConsole.SpectreConsoleAdapter)
- [`SpectreHelpProvider`](xref:AnyCli.SpectreConsole.SpectreHelpProvider), [`SpectreHelpOptions`](xref:AnyCli.SpectreConsole.SpectreHelpOptions)
- [`SpectreCommandErrorWriter`](xref:AnyCli.SpectreConsole.SpectreCommandErrorWriter), [`SpectreErrorOptions`](xref:AnyCli.SpectreConsole.SpectreErrorOptions)
- [`SpectreConsoleOptions`](xref:AnyCli.SpectreConsole.SpectreConsoleOptions)
- [`IAnsiConsoleWriter`](xref:AnyCli.SpectreConsole.IAnsiConsoleWriter), [`IAnsiConsoleOutputWriter`](xref:AnyCli.SpectreConsole.IAnsiConsoleOutputWriter), [`IAnsiConsoleErrorWriter`](xref:AnyCli.SpectreConsole.IAnsiConsoleErrorWriter)

## Related

- [The Spectre.Console adapter](../explanation/spectre-adapter.md)
- [Help and error pipeline reference](help-and-error-pipeline.md)
