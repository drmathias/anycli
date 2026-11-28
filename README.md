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
| 1 | ConsoleAppFramework | 8.356 ns | 8.336 ns | 0.009 | 0 B |
| **2** | **AnyCli** | **931.257 ns** | **930.803 ns** | **1.000** | **1344 B** |
| 3 | XenoAtom.CommandLine | 2,153.899 ns | 2,149.907 ns | 2.313 | 2992 B |
| 4 | System.CommandLine | 3,513.040 ns | 3,462.955 ns | 3.772 | 4848 B |
| 4 | CliFx | 3,817.274 ns | 3,711.786 ns | 4.099 | 4392 B |
| 5 | Spectre.Console.Cli | 141,415.089 ns | 131,008.925 ns | 151.856 | 45649 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 59.05 ms | 58.55 ms | 0.55 |
| 2 | XenoAtom.CommandLine | 91.42 ms | 91.28 ms | 0.86 |
| 3 | System.CommandLine | 97.54 ms | 97.40 ms | 0.92 |
| 4 | CliFx | 104.63 ms | 103.95 ms | 0.98 |
| **4** | **AnyCli** | **106.42 ms** | **106.22 ms** | **1.00** |
| 5 | Spectre.Console.Cli | 218.11 ms | 207.92 ms | 2.05 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | XenoAtom.CommandLine | 10.08 ms | 10.08 ms | 0.77 |
| 2 | ConsoleAppFramework | 10.56 ms | 10.54 ms | 0.80 |
| 3 | CliFx | 11.53 ms | 11.51 ms | 0.88 |
| 4 | System.CommandLine | 12.29 ms | 12.24 ms | 0.93 |
| **4** | **AnyCli** | **13.22 ms** | **12.88 ms** | **1.00** |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
