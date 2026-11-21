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
| 1 | ConsoleAppFramework | 9.169 ns | 9.183 ns | 0.008 | 0 B |
| **2** | **AnyCli** | **1,173.574 ns** | **1,131.228 ns** | **1.000** | **1320 B** |
| 3 | XenoAtom.CommandLine | 2,683.232 ns | 2,675.898 ns | 2.294 | 2992 B |
| 4 | System.CommandLine | 4,813.651 ns | 4,553.850 ns | 4.116 | 4848 B |
| 4 | CliFx | 4,892.083 ns | 4,864.132 ns | 4.183 | 4392 B |
| 5 | Spectre.Console.Cli | 128,521.274 ns | 128,171.630 ns | 109.886 | 45649 B |

### Cold start (JIT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 78.96 ms | 78.45 ms | 0.58 |
| 2 | XenoAtom.CommandLine | 115.64 ms | 115.33 ms | 0.84 |
| 3 | System.CommandLine | 128.03 ms | 126.91 ms | 0.94 |
| 3 | CliFx | 132.81 ms | 133.35 ms | 0.97 |
| **3** | **AnyCli** | **138.65 ms** | **133.86 ms** | **1.00** |
| 4 | Spectre.Console.Cli | 255.93 ms | 254.91 ms | 1.87 |

### Cold start (NativeAOT)

| Rank | Framework | Mean | Median | Ratio |
| ---: | --- | ---: | ---: | ---: |
| 1 | ConsoleAppFramework | 13.42 ms | 13.38 ms | 0.90 |
| 1 | XenoAtom.CommandLine | 13.85 ms | 13.96 ms | 0.93 |
| **1** | **AnyCli** | **15.01 ms** | **14.96 ms** | **1.00** |
| 1 | System.CommandLine | 15.77 ms | 15.64 ms | 1.06 |
| 1 | CliFx | 16.19 ms | 14.40 ms | 1.08 |

_Ranks are BenchmarkDotNet statistical ranks; ties indicate statistically similar results._
<!-- benchmarks:end -->

## Kudos

Inspiration for the framework primarily comes from both [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework) and [Spectre.Console.Cli](https://github.com/spectreconsole/spectre.console.cli).

## License

MIT
