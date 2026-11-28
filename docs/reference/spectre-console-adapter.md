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

## Adapter surface

The adapter provides:

- `context.AnsiConsole.Output` and `context.AnsiConsole.Error` on command contexts
- `context.AnsiConsole.Output` on help contexts
- `context.AnsiConsole.Error` on command error contexts
- `UseSpectreHelpRenderer` for styled help through `SpectreHelpRenderer`
- `UseSpectreErrorWriter` for styled errors through `SpectreCommandErrorWriter`

ANSI consoles are created from the existing output writers and cached per context. Raw writes through `context.Console.Output` remain available and can be mixed with Spectre output.

## Configuration boundary

`UseSpectreHelpRenderer` uses the shared `IHelpTextProvider` and `IHelpValueFormatter` configured on `ConfigureHelp`. Headings, descriptions, and default values therefore behave the same way as with the default renderer.

`UseSpectreErrorWriter` replaces only the error writer. The error message provider and exit code remain configured through `ConfigureErrorHandling`.

## Help behavior

`SpectreHelpOptions` controls Spectre-specific styles. Unless overridden, help uses aqua headings, grey option placeholders, white command placeholders, and medium-purple argument syntax. Usage lines, commands, options, and descriptions use plain styles by default.

Help rendering uses only the output stream. The underlying help context also exposes an error console because it shares the console-writer abstraction, but that stream is not part of the help pipeline.

## Error behavior

`SpectreCommandErrorWriter` renders the plain error message with Spectre styling. `SpectreErrorOptions` controls the error style and optional console options. The default error style is red, and a `null` console options value uses the error context console.

## Console options

`SpectreConsoleOptions` controls ANSI output, color-system detection, and interaction detection. By default, ANSI and color support are detected and interaction is disabled. `SpectreConsoleOptions.ForTesting` forces deterministic ANSI output with the standard color system.

## Testing

Use `SpectreConsoleOptions.ForTesting` when asserting rendered output. The adapter also supports direct console creation through `SpectreConsoleAdapter.Create` for services that do not receive a command context.

## API reference

- [`SpectreConsoleAdapter`](xref:AnyCli.SpectreConsole.SpectreConsoleAdapter)
- [`SpectreHelpRenderer`](xref:AnyCli.SpectreConsole.SpectreHelpRenderer), [`SpectreHelpOptions`](xref:AnyCli.SpectreConsole.SpectreHelpOptions)
- [`SpectreCommandErrorWriter`](xref:AnyCli.SpectreConsole.SpectreCommandErrorWriter), [`SpectreErrorOptions`](xref:AnyCli.SpectreConsole.SpectreErrorOptions)
- [`SpectreConsoleOptions`](xref:AnyCli.SpectreConsole.SpectreConsoleOptions)
- [`IAnsiConsoleWriter`](xref:AnyCli.SpectreConsole.IAnsiConsoleWriter), [`IAnsiConsoleOutputWriter`](xref:AnyCli.SpectreConsole.IAnsiConsoleOutputWriter), [`IAnsiConsoleErrorWriter`](xref:AnyCli.SpectreConsole.IAnsiConsoleErrorWriter)

## Related

- [Help reference](help.md)
- [Errors reference](errors.md)
- [Understanding help and error handling](../explanation/understanding-help-and-errors.md)
- [Styled output with Spectre.Console](../getting-started/styled-output-with-spectreconsole.md)
