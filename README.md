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
| 1 | ConsoleAppFramework | 8.220 ns | 8.220 ns | 0.008 | 0 B |
| **2** | **AnyCli** | **1,018.933 ns** | **1,016.140 ns** | **1.000** | **1336 B** |
| 3 | XenoAtom.CommandLine | 2,196.460 ns | 2,195.055 ns | 2.156 | 2992 B |
| 4 | System.CommandLine | 3,760.196 ns | 3,745.889 ns | 3.691 | 4848 B |
| 5 | CliFx | 4,107.734 ns | 4,103.297 ns | 4.033 | 4392 B |
| 6 | Spectre.Console.Cli | 106,910.011 ns | 106,759.530 ns | 104.952 | 45646 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 69.23 ms | 68.74 ms | 0.38 |
| 2 | System.CommandLine | 99.19 ms | 98.49 ms | 0.55 |
| 2 | XenoAtom.CommandLine | 102.86 ms | 102.43 ms | 0.57 |
| 3 | CliFx | 114.97 ms | 115.68 ms | 0.64 |
| **4** | **AnyCli** | **181.55 ms** | **176.30 ms** | **1.00** |
| 5 | Spectre.Console.Cli | 212.98 ms | 208.74 ms | 1.18 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | XenoAtom.CommandLine | 10.31 ms | 10.32 ms | 0.60 |
| 2 | CliFx | 12.62 ms | 12.38 ms | 0.74 |
| 2 | System.CommandLine | 13.74 ms | 13.77 ms | 0.80 |
| 2 | ConsoleAppFramework | 13.90 ms | 13.85 ms | 0.81 |
| **3** | **AnyCli** | **18.25 ms** | **16.65 ms** | **1.00** |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
