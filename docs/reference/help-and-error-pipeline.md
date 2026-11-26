<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Help and error pipeline reference

This page lists the console, help, and command-line error types used by AnyCLI.

## Console abstractions

| Type | Purpose |
| --- | --- |
| `IConsole` | Standard input, output, and error for a command. |
| `IConsoleWriter` | Standard output and error without input. |
| `IConsoleOutputWriter` | Exposes `Output` as an `IOutputWriter`. |
| `IConsoleErrorWriter` | Exposes `Error` as an `IOutputWriter`. |
| `IOutputWriter` | Write-only output: `Write`, `WriteLine()`, and `WriteLine(string?)`. |
| `IInputReader` | Reads a line from standard input with a cancellation token. |

`CommandContext.Console` is an `IConsole`. `HelpContext.Console` and `CommandErrorContext.Console` expose the narrower writer interfaces they need.

## Help pipeline

### Types

| Type | Purpose |
| --- | --- |
| `IHelpProvider` | Renders root help and command help. |
| `DefaultHelpProvider` | The built-in plain-text help renderer. |
| `IHelpTextProvider` | Supplies headings, labels, usage placeholders, descriptions, default value display text, and the formatting culture. |
| `LocalizedHelpTextProvider` | The built-in resource-based text provider. |
| `IHelpValueFormatter` | Formats fallback default values such as enums and collections. |
| `DefaultHelpValueFormatter` | The built-in formatter; uses lower kebab-case for enum members. |
| `HelpContext` | Shared help data: app name, console, and global options. |
| `RootHelpContext` | Root help data, including top-level commands. |
| `CommandHelpContext` | Command help data, including the selected command or group. |
| `HelpProviderContext` | Inputs for creating a help provider: service provider, configured text provider, value formatter, and localization. |
| `HelpDescriptionContext` | Inputs for resolving a command, option, or argument description. |
| `HelpText` | Literal text, a resource key, or both. |
| `ResourceDescriptionAttribute` | Associates a parameter property with a resource key. |
| `DefaultValueDisplayAttribute` | Supplies literal default value display text for help output. |
| `DefaultValueDisplayResourceAttribute` | Supplies a localizable default value display resource key. |
| `DefaultValueInfo` | Pattern-matchable default source: initializer value or provider type, with optional display text. |

### Help configuration

`ConfigureHelp` configures shared help services and selects the help provider:

| Method | Effect |
| --- | --- |
| `UseTextProvider<TProvider>()` | Replaces the help text provider. |
| `UseLocalizedTextProvider(params ResourceManager[])` | Uses `LocalizedHelpTextProvider` with description resources. |
| `UseLocalizedTextProvider(Func<IServiceProvider?, IEnumerable<ResourceManager>>)` | Creates description resources through a factory. |
| `UseLocalizedTextProvider(string baseName, Assembly assembly)` | Creates a `ResourceManager` from a base name and assembly. |
| `UseValueFormatter<TFormatter>()` | Replaces the default value formatter. |
| `UseValueFormatter(IHelpValueFormatter)` | Uses the specified value formatter instance. |
| `UseValueFormatter(Func<IServiceProvider?, IHelpValueFormatter>)` | Creates the value formatter through a factory. |
| `UseProvider<TProvider>()` | Replaces the help renderer. |
| `UseProvider(Func<HelpProviderContext, IHelpProvider>)` | Creates the renderer with access to services and configured help services. |

The configured text provider and value formatter apply to every provider factory through `HelpProviderContext`. This includes custom providers.

When no help provider is configured, `DefaultHelpProvider` is used. When no help text provider is configured, `LocalizedHelpTextProvider` is used. When no value formatter is configured, `DefaultHelpValueFormatter` is used.

A help provider resolved with `UseProvider<TProvider>()` may be resolved from DI when a service provider is configured. Otherwise it needs a public parameterless constructor. Use `UseProvider(Func<HelpProviderContext, IHelpProvider>)` when a custom provider needs the configured help services.

### Resource keys

`HelpTextResourceKeys` defines the keys used for built-in headings, labels, and placeholders:

- `Usage`
- `Description`
- `Commands`
- `Arguments`
- `Options`
- `DefaultValue`
- `OptionsPlaceholder`
- `CommandPlaceholder`
- `HelpOptionDescription`
- `VersionOptionDescription`

## Error pipeline

Invalid command-line input is represented by `CommandError` and thrown as `CommandErrorException`. `ConsoleApp.RunAsync` catches that exception and delegates it to an `ICommandErrorHandler`.

### Structured errors

| Error | Data |
| --- | --- |
| `NoCommandSpecifiedError` | No additional data. |
| `UnknownCommandError` | `AttemptedCommandPath`, `AvailableCommands` |
| `MissingSubcommandError` | `CommandPath`, `AvailableCommands` |
| `UnknownOptionError` | `OptionName` |
| `MalformedOptionError` | `Token` |
| `OptionRequiresSeparateValueError` | `OptionName` |
| `MissingOptionValueError` | `OptionName` |
| `RepeatedOptionError` | `OptionName` |
| `MissingRequiredOptionError` | `OptionName`, `IsGlobal` |
| `MissingRequiredArgumentError` | `ArgumentName` |
| `UnexpectedArgumentError` | `Value` |
| `TooFewVariadicValuesError` | `ArgumentName`, `Minimum`, `Actual` |
| `TooManyVariadicValuesError` | `ArgumentName`, `Maximum`, `Actual` |
| `InvalidParameterValueError` | `ParameterName`, `Value`, `TargetType`, `Source` |

`ParameterSource` identifies whether an invalid value came from an argument, option, or global option.

### Error types

| Type | Purpose |
| --- | --- |
| `CommandError` | Base type for structured command-line errors. |
| `CommandErrorException` | Carries a `CommandError` and command path. |
| `CommandErrorContext` | App name, command path, structured error, and error console. |
| `ICommandErrorHandler` | Handles the command-line error response. |
| `DefaultCommandErrorHandler` | Combines a message provider and writer. |
| `ICommandErrorMessageProvider` | Produces the plain-text message. |
| `LocalizedCommandErrorMessageProvider` | Built-in resource-based message provider. |
| `ICommandErrorWriter` | Writes the message to standard error. |
| `PlainCommandErrorWriter` | Built-in unstyled writer. |

### Error configuration

| Method | Effect |
| --- | --- |
| `UseHandler<THandler>()` | Replaces the error handler. |
| `UseMessageProvider<TProvider>()` | Replaces the message provider. |
| `UseLocalizedMessageProvider(...)` | Uses `LocalizedCommandErrorMessageProvider` with resources. |
| `UseWriter<TWriter>()` | Replaces the error writer. |
| `UseWriter(Func<IServiceProvider?, ICommandErrorWriter>)` | Creates the writer with access to services. |
| `UseExitCode(int)` | Sets a fixed command-line error exit code. |
| `UseExitCode<TExitCode>(TExitCode)` | Sets a fixed exit code from an enum. |
| `UseExitCode(Func<CommandErrorContext, int>)` | Derives the exit code from the error context. |

The default exit code for command-line errors is `2`.

`CommandErrorResourceKeys` defines the keys used by the built-in localized error messages.

## Scope

The error pipeline handles command-line input errors. Exceptions thrown by command bodies are not converted into `CommandError` values. Cancellation is also separate and propagates from `RunAsync`.

## Related

- [Help, errors, and output](../explanation/help-errors-and-output.md)
- [Spectre.Console adapter reference](spectre-console-adapter.md)
