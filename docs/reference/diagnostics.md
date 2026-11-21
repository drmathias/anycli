<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Diagnostics reference

The AnyCli source generator and analyzer emit the following compile-time diagnostics. All are errors and are enabled by default.

| ID | Diagnostic | Cause and fix |
| --- | --- | --- |
| `ANYCLI001` | Unsupported parameter type | The property type is not `string`, an enum, `ISpanParsable<T>`, or `IParsable<T>`, and no parser is configured. Implement a supported parsing interface, use an enum, set `Parser`, or register a `ParameterValueParserAttribute`. |
| `ANYCLI002` | Invalid parameter parser | The configured `Parser` type does not implement `IParameterParser<T>` for the property's value type. Make the parser implement the matching closed interface. |
| `ANYCLI003` | Invalid variadic parameter | A variadic argument or repeated option uses an unsupported collection type. Use an array or one of the supported collection shapes. |
| `ANYCLI004` | Duplicate parameter parser registration | More than one `ParameterValueParserAttribute` is registered for the same value type. Remove all but one registration. |
| `ANYCLI005` | Invalid variadic minimum count | `MinimumCount` is negative. Use zero or a positive value. |
| `ANYCLI006` | Invalid default value type | The configured `DefaultValue` cannot be assigned to the property type. Use a compatible value or property initializer. |
| `ANYCLI007` | Invalid argument position | Argument positions must be unique, non-negative, and contiguous starting at `0`. Correct the `Argument` or `OptionalArgument` positions. |
| `ANYCLI008` | Unsupported default value | The configured `DefaultValue` is not a supported constant. Use a compile-time constant that is valid for the parameter type. |
| `ANYCLI009` | Invalid option name | An option name is not a valid short name such as `-o` or long name such as `--output`. Correct the `Option` name. |
| `ANYCLI010` | Invalid collection default value | A collection parameter's `DefaultValue` is not a non-null array. Use an array value. |
| `ANYCLI011` | Invalid default value element type | A collection default contains an element that cannot be assigned to the collection's element type. Make every element compatible. |
| `ANYCLI012` | Default values on required variadic parameter | A variadic parameter with `MinimumCount` greater than zero also has a default value. Remove the default or set `MinimumCount = 0`. |
| `ANYCLI013` | Invalid option alias | An option alias is not a valid short name such as `-o` or long name such as `--output`. Correct the alias. |
| `ANYCLI014` | Duplicate option name | An option name or alias is reused within the parameter type. Make every option name and alias unique. |
| `ANYCLI015` | Global option declared as argument | A property on a `[GlobalOptions]` type uses `[Argument]`. Global options must use `[Option]`. |
| `ANYCLI016` | Conflicting parameters kind | A type is marked `[GlobalOptions]` and also implements `ICommandParameters<T>`. Use one role; command parameter types should inherit from the global options type instead. |
| `ANYCLI017` | Invalid variadic maximum count | `MaximumCount` must be greater than `1` and at least `MinimumCount`. Raise the maximum or use a scalar parameter instead. |
| `ANYCLI018` | Interceptor global options mismatch | An interceptor requires typed global options that the application does not configure. Call `AddGlobalOptions<TGlobalOptions>()` with the required type on the application builder. |
| `ANYCLI019` | Interceptor exit code mismatch | An interceptor requires a command exit code type that the application does not configure. Call `ConfigureCommands<TExitCode>(...)` with the required type on the application builder. |

Parameter diagnostics are emitted by the source generator before parameter code is generated. Interceptor diagnostics are reported on the `AddInterceptor<T>()` call. Fixing the diagnostic is required for the application to compile.
