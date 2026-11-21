<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

![AnyCli](branding/anycli-readme.png)

AnyCli is a source-generated, AOT-friendly command-line framework for .NET with typed command parameters.

[![Tests](https://github.com/drmathias/anycli/actions/workflows/test.yml/badge.svg)](https://github.com/drmathias/anycli/actions/workflows/test.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/drmathias/anycli/blob/main/LICENSES/MIT.txt)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4)](https://dotnet.microsoft.com/)
[![.NET 11](https://img.shields.io/badge/.NET-11-512BD4)](https://dotnet.microsoft.com/)
[![AOT ready](https://img.shields.io/badge/AOT-ready-brightgreen)](https://learn.microsoft.com/dotnet/core/deploying/native-aot/)

Read the [documentation](https://drmathias.github.io/anycli/) for getting started, guides, and the API reference.

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
- Nested command groups
- Custom value parsers
- Dependency injection support
- Typed enum exit codes
- Typed global options
- Interceptor middleware
- Cancellation support for async commands and interceptors
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
| 1 | ConsoleAppFramework | 9.034 ns | 8.737 ns | 0.009 | 0 B |
| **2** | **AnyCli** | **969.983 ns** | **951.020 ns** | **1.000** | **1320 B** |
| 3 | XenoAtom.CommandLine | 2,234.497 ns | 2,250.428 ns | 2.311 | 2992 B |
| 4 | System.CommandLine | 3,858.519 ns | 3,907.955 ns | 3.990 | 4848 B |
| 5 | CliFx | 4,428.909 ns | 4,478.285 ns | 4.580 | 4392 B |
| 6 | Spectre.Console.Cli | 108,867.854 ns | 108,747.203 ns | 112.592 | 45646 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 62.80 ms | 62.32 ms | 0.56 |
| **2** | **AnyCli** | **113.17 ms** | **114.48 ms** | **1.00** |
| 2 | CliFx | 114.59 ms | 115.89 ms | 1.02 |
| 2 | XenoAtom.CommandLine | 118.06 ms | 117.32 ms | 1.05 |
| 2 | System.CommandLine | 127.33 ms | 125.16 ms | 1.13 |
| 3 | Spectre.Console.Cli | 216.18 ms | 216.13 ms | 1.92 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| **1** | **AnyCli** | **11.70 ms** | **11.59 ms** | **1.00** |
| 1 | XenoAtom.CommandLine | 12.22 ms | 11.73 ms | 1.05 |
| 1 | ConsoleAppFramework | 12.66 ms | 12.62 ms | 1.08 |
| 1 | CliFx | 13.77 ms | 13.71 ms | 1.18 |
| 1 | System.CommandLine | 14.05 ms | 13.78 ms | 1.20 |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
