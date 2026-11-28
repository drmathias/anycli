<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

![AnyCli](branding/anycli-readme.png)

AnyCli is a source-generated, AOT-friendly command-line framework for .NET with typed command parameters.

Read the [documentation](https://drmathias.github.io/anycli/) for getting started, guides, and the API reference.

[![Tests](https://github.com/drmathias/anycli/actions/workflows/test.yml/badge.svg)](https://github.com/drmathias/anycli/actions/workflows/test.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/drmathias/anycli/blob/main/LICENSES/MIT.txt)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![.NET 11](https://img.shields.io/badge/.NET-11-512BD4)](https://dotnet.microsoft.com/)
[![AOT ready](https://img.shields.io/badge/AOT-ready-brightgreen)](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)

## Philosophy

AnyCli is optimised for both utility and extensibility. It is mildly opinionated and suitable for building almost all types of CLI apps. If your use case isn't supported, please open an issue.

## Packages

| Package | Version | Description |
| --- | --- | --- |
| `AnyCli` | [![NuGet](https://img.shields.io/nuget/vpre/AnyCli?logo=nuget)](https://www.nuget.org/packages/AnyCli) | Core command-line framework and source generator. |
| `AnyCli.SpectreConsole` | [![NuGet](https://img.shields.io/nuget/vpre/AnyCli.SpectreConsole?logo=nuget)](https://www.nuget.org/packages/AnyCli.SpectreConsole) | Spectre.Console integration for ANSI command output. |

## Features

- AOT-compatible command registration and execution
- Source-generated parameter binding with no runtime reflection
- Typed positional arguments and named options
- Required, optional, variadic, collection, and defaulted parameters
- Nested command groups and executable commands with subcommands
- Custom value parsers
- Custom default value providers
- Dependency injection support
- Typed enum exit codes
- Typed global options
- Interceptor middleware
- Full async and cancellation support for commands, parsers, default value providers and interceptors
- Fully customisable help and error handling
- Built in internationalisation - localized error messages and help for English, French, German, Spanish, Brazilian Portuguese, Italian, and Dutch
- Configurable localization
- Optional Spectre.Console adapter for rich, styled terminal output and error messages (tables, markup, progress bars etc.)

## Installation

```bash
dotnet add package AnyCli
```

For Spectre.Console integration:

```bash
dotnet add package AnyCli.SpectreConsole
```

## Benchmarks

<!-- benchmarks:start -->
_Environment: macOS Tahoe 26.2, Intel Core i7-9750H, 12 logical cores, .NET SDK 11.0.100, .NET 11.0.0, X64._

### Invocation

| Rank | Framework | Mean | Median | Ratio | Allocated |
| ---: | --- | ---: | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 8.254 ns | 8.149 ns | 0.008 | 0 B |
| **2** | **AnyCli** | **992.604 ns** | **966.115 ns** | **1.000** | **1344 B** |
| 3 | XenoAtom.CommandLine | 2,192.567 ns | 2,177.958 ns | 2.216 | 2992 B |
| 4 | CliFx | 3,738.693 ns | 3,690.362 ns | 3.778 | 4392 B |
| 4 | System.CommandLine | 3,900.972 ns | 3,840.765 ns | 3.942 | 4848 B |
| 5 | Spectre.Console.Cli | 108,010.919 ns | 106,767.499 ns | 109.141 | 45646 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 58.38 ms | 58.39 ms | 0.55 |
| 2 | XenoAtom.CommandLine | 95.26 ms | 94.19 ms | 0.89 |
| 2 | System.CommandLine | 97.80 ms | 97.60 ms | 0.91 |
| **3** | **AnyCli** | **107.02 ms** | **107.04 ms** | **1.00** |
| 3 | CliFx | 112.06 ms | 112.37 ms | 1.05 |
| 4 | Spectre.Console.Cli | 208.02 ms | 206.31 ms | 1.94 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | XenoAtom.CommandLine | 10.02 ms | 9.998 ms | 0.83 |
| 2 | ConsoleAppFramework | 10.69 ms | 10.494 ms | 0.88 |
| 3 | CliFx | 12.02 ms | 11.856 ms | 0.99 |
| **3** | **AnyCli** | **12.22 ms** | **11.574 ms** | **1.00** |
| 3 | System.CommandLine | 12.28 ms | 12.303 ms | 1.02 |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
