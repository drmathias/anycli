<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Diagnostics reference

| ID | Severity | Diagnostic | Cause and fix |
| --- | --- | --- | --- |
| [`ANYCLI001`](diagnostics/anycli001.md) | Error | Unsupported parameter type | The property type is not `string`, an enum, `ISpanParsable<T>`, or `IParsable<T>`, and no parser is configured. Implement a supported parsing interface, use an enum, set `Parser`, or register a `ParameterValueParserAttribute`. |
| [`ANYCLI002`](diagnostics/anycli002.md) | Error | Invalid parameter parser | The configured `Parser` type does not implement `IParameterParser<T>` or `IAsyncParameterParser<T>` for the property's type. Make the parser implement the matching closed interface. |
| [`ANYCLI003`](diagnostics/anycli003.md) | Error | Invalid variadic parameter | A variadic argument or repeated option uses an unsupported collection type. Use an array or one of the supported collection shapes. |
| [`ANYCLI004`](diagnostics/anycli004.md) | Warning | Duplicate parameter parser registration | More than one parser is registered for the same type. The last registration overrides the previous one; remove duplicates to silence the warning. |
| [`ANYCLI005`](diagnostics/anycli005.md) | Error | Invalid variadic minimum count | `MinimumCount` is negative. Use zero or a positive value. |
| [`ANYCLI007`](diagnostics/anycli007.md) | Error | Invalid argument position | Argument positions must be unique, non-negative, and contiguous starting at `0`. Correct the `Argument` or `OptionalArgument` positions. |
| [`ANYCLI009`](diagnostics/anycli009.md) | Error | Invalid option name | An option name is not a valid short name such as `-o` or long name such as `--output`. Correct the `Option` name. |
| [`ANYCLI013`](diagnostics/anycli013.md) | Error | Invalid option alias | An option alias is not a valid short name such as `-o` or long name such as `--output`. Correct the alias. |
| [`ANYCLI014`](diagnostics/anycli014.md) | Error | Duplicate option name | An option name or alias is reused within the parameter type. Make every option name and alias unique. |
| [`ANYCLI015`](diagnostics/anycli015.md) | Error | Global option declared as argument | A property on a `[GlobalOptions]` type uses `[Argument]`. Global options must use `[Option]`. |
| [`ANYCLI016`](diagnostics/anycli016.md) | Error | Conflicting parameters kind | A type is marked `[GlobalOptions]` and also implements `ICommandParameters<T>`. Use one role; command parameter types should inherit from the global options type instead. |
| [`ANYCLI017`](diagnostics/anycli017.md) | Error | Invalid variadic maximum count | `MaximumCount` must be greater than `1` and at least `MinimumCount`. Raise the maximum or use a scalar parameter instead. |
| [`ANYCLI018`](diagnostics/anycli018.md) | Error | Interceptor global options mismatch | An interceptor requires typed global options that the application does not configure. Call `AddGlobalOptions<TGlobalOptions>()` with the required type on the application builder. |
| [`ANYCLI019`](diagnostics/anycli019.md) | Error | Interceptor exit code mismatch | An interceptor requires a command exit code type that the application does not configure. Call `ConfigureCommands<TExitCode>(...)` with the required type on the application builder. |
| [`ANYCLI020`](diagnostics/anycli020.md) | Error | Invalid default value provider | A `DefaultValueProvider` type is not a non-abstract class implementing `IDefaultValueProvider<T>` or `IAsyncDefaultValueProvider<T>` for the parameter property type. Use a compatible provider type. |
| [`ANYCLI022`](diagnostics/anycli022.md) | Warning | Help default value on required parameter | A required parameter has `HelpDefaultValue` metadata. The metadata is ignored; remove it or make the parameter optional. |
| [`ANYCLI023`](diagnostics/anycli023.md) | Warning | Help default value without default value | A parameter has `HelpDefaultValue` metadata but no default provider or property initializer. The metadata is ignored; add a default source or remove it. |
| [`ANYCLI025`](diagnostics/anycli025.md) | Warning | Ambiguous default value provider | A provider type implements both `IDefaultValueProvider<T>` and `IAsyncDefaultValueProvider<T>`. The async provider is used; remove one implementation to silence the warning. |
| [`ANYCLI026`](diagnostics/anycli026.md) | Warning | Ambiguous parameter parser | A parser type implements both sync and async parser interfaces for the parameter type. Async parsing is preferred; remove one implementation to silence the warning. |
