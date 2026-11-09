<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# AnyCli

AnyCli is a source-generated, AOT-friendly command-line framework for .NET with typed command parameters.

## Philosophy

AnyCli is optimised for both utility and extensibility. It is moderately opinionated and intended to be suitable to building any command line application. If your use case isn't supported, open an issue.

## Packages

| Package | Description |
| --- | --- |
| `AnyCli` | Core command-line framework and source generator. |
| `AnyCli.SpectreConsole` | Spectre.Console integration for ANSI command output. |

## Features

- AOT-compatible command registration and execution
- Source-generated parameter binding with no runtime reflection
- Typed positional arguments and named options
- Required, optional, variadic, collection, and defaulted parameters
- Nested command groups
- Custom value parsers
- Dependency injection support
- Global options
- Interceptor middleware
- Customisable help provider

## Installation

```bash
dotnet add package AnyCli
```

For Spectre.Console integration:

```bash
dotnet add package AnyCli.SpectreConsole
```

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
