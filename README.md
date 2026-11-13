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
- Typed enum exit codes
- Typed global options
- Interceptor middleware
- Cancellation support for async commands and interceptors
- Fully customisable help and error handling
- Built in internationalisation - localized error messages for English, French, German, Spanish, Brazilian Portuguese, Italian, and Dutch
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
_Environment: macOS Tahoe 26.2, Intel Core i7-9750H, 12 logical cores, .NET SDK 10.0.403, .NET 10.0.14, X64._

### Invocation

| Rank | Framework | Mean | Median | Ratio | Allocated |
| ---: | --- | ---: | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 2.468 ns | 2.419 ns | 0.002 | 0 B |
| **2** | **AnyCli** | **1,288.072 ns** | **1,268.416 ns** | **1.001** | **2232 B** |
| 3 | System.CommandLine | 3,399.265 ns | 3,398.128 ns | 2.641 | 4864 B |
| 4 | Spectre.Console.Cli | 108,885.945 ns | 106,483.853 ns | 84.601 | 45920 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 343.0 ms | 343.8 ms | 0.95 |
| **1** | **AnyCli** | **360.6 ms** | **362.8 ms** | **1.00** |
| 1 | System.CommandLine | 365.2 ms | 365.9 ms | 1.01 |
| 2 | Spectre.Console.Cli | 502.5 ms | 504.3 ms | 1.39 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| **1** | **AnyCli** | **251.7 ms** | **251.0 ms** | **1.01** |
| 1 | ConsoleAppFramework | 276.0 ms | 276.7 ms | 1.11 |
| 1 | System.CommandLine | 276.3 ms | 276.7 ms | 1.11 |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
