<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# AnyCli

AnyCli is a source-generated, AOT-friendly command-line framework for .NET with typed command parameters.

[![Tests](https://github.com/drmathias/anycli/actions/workflows/test.yml/badge.svg)](https://github.com/drmathias/anycli/actions/workflows/test.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/drmathias/anycli/blob/main/LICENSES/MIT.txt)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
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
- Typed global options
- Interceptor middleware
- Cancellation support for async commands and interceptors
- Fully customisable help provider
- Optional Spectre.Console adapter for rich, styled terminal output (tables, markup, progress bars etc.)

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
_Environment: macOS Tahoe 26.2, Intel Core i7-9750H, 12 logical cores, .NET SDK 10.0.403, .NET 10.0.14, X64._

### Invocation

| Rank | Framework | Mean | Median | Ratio | Allocated |
| ---: | --- | ---: | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 2.719 ns | 2.711 ns | 0.002 | 0 B |
| **2** | **AnyCli** | **1,117.709 ns** | **1,116.287 ns** | **1.000** | **2232 B** |
| 3 | System.CommandLine | 3,182.558 ns | 3,178.387 ns | 2.847 | 4864 B |
| 4 | Spectre.Console.Cli | 102,019.628 ns | 102,088.674 ns | 91.279 | 45920 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 305.0 ms | 305.4 ms | 0.87 |
| 2 | System.CommandLine | 343.6 ms | 342.2 ms | 0.98 |
| **2** | **AnyCli** | **350.7 ms** | **351.4 ms** | **1.00** |
| 3 | Spectre.Console.Cli | 450.8 ms | 448.5 ms | 1.29 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | System.CommandLine | 231.4 ms | 230.8 ms | 0.94 |
| **1** | **AnyCli** | **247.1 ms** | **248.2 ms** | **1.00** |
| 1 | ConsoleAppFramework | 273.3 ms | 275.7 ms | 1.11 |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
