<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Spectre.Console adapter reference

The `AnyCli.SpectreConsole` package connects AnyCLI contexts to Spectre.Console. Install it with:

```bash
dotnet add package AnyCli.SpectreConsole
```

Use the `AnyCli.SpectreConsole` namespace for the adapter, providers, writers, and options.

## Adapter types

| Type | Purpose |
| --- | --- |
| `SpectreConsoleAdapter` | Creates ANSI consoles and exposes cached consoles for AnyCLI contexts. |
| `IAnsiConsoleWriter` | Provides `Output` and `Error` consoles. |
| `IAnsiConsoleOutputWriter` | Provides the standard output console. |
| `IAnsiConsoleErrorWriter` | Provides the standard error console. |
| `SpectreHelpProvider` | Renders help with Spectre.Console. |
| `SpectreCommandErrorWriter` | Renders command-line errors with Spectre.Console. |
| `SpectreConsoleOptions` | Controls ANSI, color, and interaction detection. |
| `SpectreHelpOptions` | Controls help styles. |
| `SpectreErrorOptions` | Controls error styles and console options. |

## Adapter methods

| Method | Returns |
| --- | --- |
| `SpectreConsoleAdapter.Create(IOutputWriter, SpectreConsoleOptions?)` | `IAnsiConsole` for an output writer. |
| `SpectreConsoleAdapter.Create(IConsole, SpectreConsoleOptions?)` | `IAnsiConsole` for a console's standard output. |
| `SpectreConsoleAdapter.CreateError(IConsole, SpectreConsoleOptions?)` | `IAnsiConsole` for a console's standard error. |

## Context extensions

| Context | Intended property path | Type |
| --- | --- | --- |
| `CommandContext` | `AnsiConsole.Output`, `AnsiConsole.Error` | `IAnsiConsoleWriter` |
| `HelpContext` | `AnsiConsole.Output` | `IAnsiConsoleWriter` |
| `CommandErrorContext` | `AnsiConsole.Error` | `IAnsiConsoleErrorWriter` |

Help rendering only uses `Output`. The underlying `HelpContext.AnsiConsole` type also exposes `Error` because it shares the console-writer abstraction, but that stream is not part of the help pipeline.

`CommandContext.AnsiConsole.Output` and `.Error` are cached for the lifetime of the context. Raw writes through `CommandContext.Console` remain available and can be mixed with ANSI output.

## Configuration extensions

| Method | Effect |
| --- | --- |
| `UseSpectreHelp(SpectreHelpOptions?)` | Replaces the help renderer with `SpectreHelpProvider`. |
| `UseSpectreErrors(SpectreErrorOptions?)` | Replaces the error writer with `SpectreCommandErrorWriter`. |

`UseSpectreHelp` uses the configured `IHelpTextProvider` from `HelpProviderContext`, so headings and descriptions remain localizable. `UseSpectreErrors` replaces only the writer; the message provider and exit code remain unchanged.

## SpectreConsoleOptions

| Property | Default | Purpose |
| --- | --- | --- |
| `Ansi` | `AnsiSupport.Detect` | Whether ANSI output is enabled. |
| `ColorSystem` | `ColorSystemSupport.Detect` | Color system detection. |
| `Interactive` | `InteractionSupport.No` | Interactive terminal behavior. |

`SpectreConsoleOptions.ForTesting` uses `AnsiSupport.Yes`, `ColorSystemSupport.Standard`, and `InteractionSupport.No`.

## SpectreHelpOptions

| Property | Default |
| --- | --- |
| `HeadingStyle` | `Color.Aqua` |
| `UsageStyle` | `Style.Plain` |
| `OptionsPlaceholderStyle` | `Color.Grey` |
| `CommandPlaceholderStyle` | `Color.White` |
| `CommandStyle` | `Style.Plain` |
| `OptionStyle` | `Style.Plain` |
| `ArgumentStyle` | `Color.MediumPurple1` |
| `DescriptionStyle` | `Style.Plain` |

## SpectreErrorOptions

| Property | Default | Purpose |
| --- | --- | --- |
| `ErrorStyle` | `Color.Red` | Style applied to the error message. |
| `ConsoleOptions` | `null` | Console options; `null` uses the error context console. |

## Related

- [The Spectre.Console adapter](../explanation/spectre-adapter.md)
- [Help and error pipeline reference](help-and-error-pipeline.md)
