<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# The Spectre.Console adapter

The `AnyCli.SpectreConsole` package adds rich terminal output without changing how commands, help text, or error messages work.

## What the adapter adds

The adapter connects AnyCLI's console abstractions to Spectre.Console:

- `context.AnsiConsole.Output` and `context.AnsiConsole.Error` on command contexts
- `context.AnsiConsole.Output` on help contexts
- `context.AnsiConsole.Error` on command error contexts
- `SpectreHelpProvider`, enabled with `UseSpectreHelp`
- `SpectreCommandErrorWriter`, enabled with `UseSpectreErrors`

The ANSI consoles are created from the existing output writers and cached per context. Raw writes through `context.Console.Output` still work and can be mixed with Spectre output.

## What it does not replace

The adapter is a presentation layer. It does not replace:

- the help text provider that supplies headings, descriptions, and localization
- the error message provider that turns structured errors into text
- the error exit-code configuration
- command registration or parameter binding

`UseSpectreHelp` replaces the help renderer but uses the configured `IHelpTextProvider`. `UseSpectreErrors` replaces only the error writer; the message provider and exit code still come from the core error pipeline.

That separation means localization, message changes, and exit codes keep working when the adapter is enabled.

## When to use it

Use the adapter when terminal output is meant for people:

- tables, panels, markup, progress, and status displays
- styled help output
- styled command-line errors
- consistent color and emphasis across commands

Keep machine-readable output plain. A command that prints a filesystem path, JSON, or another value intended for scripts should not wrap that value in markup or ANSI styling — use `Console.Output` instead.

## Detection and testing

By default, the adapter detects ANSI support and the available color system. This allows the same command to run in an interactive terminal and in redirected output.

For tests, use `SpectreConsoleOptions.ForTesting` to force deterministic ANSI output and a standard color system. The adapter also supports direct console creation through `SpectreConsoleAdapter.Create`, which is useful for services that do not receive a command context.

## Related

- [Render rich output](../guides/spectre/render-rich-output.md)
- [Use Spectre.Console help](../guides/spectre/use-spectre-help.md)
- [Use Spectre.Console errors](../guides/spectre/use-spectre-errors.md)
- [Spectre.Console adapter reference](../reference/spectre-console-adapter.md)
