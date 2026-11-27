<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Diagnostics reference

The AnyCli source generator and analyzer emit the following compile-time diagnostics. All are errors and are enabled by default.

| ID | Diagnostic | Cause and fix |
| --- | --- | --- |
| `ANYCLI001` | Unsupported parameter type | The property type is not `string`, an enum, `ISpanParsable<T>`, or `IParsable<T>`, and no parser is configured. Implement a supported parsing interface, use an enum, set `Parser`, or register a `ParameterValueParserAttribute`. |
| `ANYCLI002` | Invalid parameter parser | The configured `Parser` type does not implement `IParameterParser<T>` or `IAsyncParameterParser<T>` for the property's value type. Make the parser implement the matching closed interface. |
| `ANYCLI003` | Invalid variadic parameter | A variadic argument or repeated option uses an unsupported collection type. Use an array or one of the supported collection shapes. |
| `ANYCLI004` | Duplicate parameter parser registration | More than one parser is registered for the same value type. Remove all but one registration. |
| `ANYCLI005` | Invalid variadic minimum count | `MinimumCount` is negative. Use zero or a positive value. |
| `ANYCLI007` | Invalid argument position | Argument positions must be unique, non-negative, and contiguous starting at `0`. Correct the `Argument` or `OptionalArgument` positions. |
| `ANYCLI009` | Invalid option name | An option name is not a valid short name such as `-o` or long name such as `--output`. Correct the `Option` name. |
| `ANYCLI013` | Invalid option alias | An option alias is not a valid short name such as `-o` or long name such as `--output`. Correct the alias. |
| `ANYCLI014` | Duplicate option name | An option name or alias is reused within the parameter type. Make every option name and alias unique. |
| `ANYCLI015` | Global option declared as argument | A property on a `[GlobalOptions]` type uses `[Argument]`. Global options must use `[Option]`. |
| `ANYCLI016` | Conflicting parameters kind | A type is marked `[GlobalOptions]` and also implements `ICommandParameters<T>`. Use one role; command parameter types should inherit from the global options type instead. |
| `ANYCLI017` | Invalid variadic maximum count | `MaximumCount` must be greater than `1` and at least `MinimumCount`. Raise the maximum or use a scalar parameter instead. |
| `ANYCLI018` | Interceptor global options mismatch | An interceptor requires typed global options that the application does not configure. Call `AddGlobalOptions<TGlobalOptions>()` with the required type on the application builder. |
| `ANYCLI019` | Interceptor exit code mismatch | An interceptor requires a command exit code type that the application does not configure. Call `ConfigureCommands<TExitCode>(...)` with the required type on the application builder. |
| `ANYCLI020` | Invalid default value provider | A `DefaultValueProvider` type is not a non-abstract class implementing `IDefaultValueProvider<T>` or `IAsyncDefaultValueProvider<T>` for the parameter property type. Use a compatible provider type. |
| `ANYCLI022` | Default value display on required parameter | A required argument, required option, or required variadic parameter has `DefaultValueDisplay` or `DefaultValueDisplayResource`. Default display metadata is only valid for optional parameters. |
| `ANYCLI023` | Default value display without default value | A parameter has default display metadata but no provider or property initializer. Add a default or remove the display metadata. |
| `ANYCLI025` | Ambiguous default value provider | A provider type implements both `IDefaultValueProvider<T>` and `IAsyncDefaultValueProvider<T>`. Implement only one interface. |
| `ANYCLI026` | Ambiguous parameter parser | A parser type implements both the sync and async parser interfaces for the parameter value type. Implement only one interface. |

Parameter diagnostics are emitted by the source generator before parameter code is generated. Interceptor diagnostics are reported on the `AddInterceptor<T>()` call. Fixing the diagnostic is required for the application to compile.
