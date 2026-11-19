<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Command-line syntax

This page summarizes the command-line forms AnyCLI accepts.

## Arguments

Arguments are positional values and are matched in declaration order:

```bash
copy input.txt output.txt
```

A required argument must be present. An optional argument can be omitted. Variadic arguments consume the remaining positional values up to their configured maximum.

## Options

Options can use separate values or `=`:

```bash
list --status open
list --status=open
list -s open
list -s=open
```

Short flag options can be grouped:

```bash
run -vq
```

Grouped short options that take values are not supported; supply value-taking short options separately.

## Boolean options

A boolean option is a flag:

```bash
run --verbose
run --verbose=false
```

## End of options

Use `--` to stop option parsing. Remaining tokens are treated as positional values:

```bash
run -- --literal-value
```

## Global options

Global options can appear before or after the command name:

```bash
todo --log-level debug list
todo list --log-level debug
```

## Help and version

`-h` and `--help` display help. When an application version is configured, `-v` and `--version` display it.

## Errors

Command-line parsing failures are structured errors. By default they are written to standard error and return exit code `2`. Error output and exit codes can be customized through `ConfigureErrorHandling`.

## Related

- [Parameter attributes](parameter-attributes.md)
- [Arguments and options tutorial](../getting-started/arguments-and-options.md)
