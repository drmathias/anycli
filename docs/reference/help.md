<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Help reference

This page describes the help metadata, providers, and configuration used by AnyCLI. For exact types, members, and signatures, use the API Reference in the top navigation.

## Entry points

AnyCLI provides:

- root help through `-h` and `--help`
- command help through `-h` and `--help` after a command path
- version output through `-v` and `--version` when a version is configured

Help is written to standard output through an `IConsoleWriter`.

## Metadata and contexts

Help is built from `ApplicationInfo` and `CommandInfo` metadata. `ApplicationInfo` contains:

- the application name
- application-declared global options
- framework-provided options such as help and version
- the root command tree

Root help receives `RootHelpContext`. Command help receives `CommandHelpContext`, which adds the selected command or command group. Both contexts expose the shared `ApplicationInfo` and console writer.

`CommandInfo` contains command-specific arguments, options, and children. Global and framework options remain application-level metadata.

## Help renderer

`IHelpRenderer` receives a help context and writes the output. `DefaultHelpRenderer` is the built-in plain-text renderer.

`ConfigureHelp` can select a renderer with `UseRenderer<TRenderer>()` or a factory. A renderer registered in the configured service provider is resolved from it; otherwise, a public parameterless constructor is used. When no renderer is configured, `DefaultHelpRenderer` is used.

A custom renderer factory receives `HelpRendererContext`, which contains:

- the application service provider, when configured
- the configured `IHelpTextProvider`
- the configured `IHelpValueFormatter`
- the configured localization options

## Text and values

`IHelpTextProvider` supplies headings, labels, placeholders, and descriptions. The default is `LocalizedHelpTextProvider`.

`IHelpValueFormatter` formats fallback default values such as enums and collections. The default is `DefaultHelpValueFormatter`.

Use `ConfigureHelp` with `UseTextProvider`, `UseLocalizedTextProvider`, or `UseValueFormatter` to replace these services. Each leaf component supports type-based resolution through DI or a parameterless constructor, and an instance overload for a preconfigured component. The localized-resource overloads create a localized provider from resources.

Help renderers use a different rule: `UseRenderer<TRenderer>()` resolves through DI, while `UseRenderer(Func<HelpRendererContext, IHelpRenderer>)` receives the configured text provider, value formatter, localization, and service provider.

Parameter descriptions use `[HelpDescription]`. Default value display text uses `[HelpDefaultValue]`.

`[HelpDefaultValue]` affects help output only. It does not change binding and does not invoke a default value provider. Built-in resource keys are exposed by `HelpTextResourceKeys`.

## API reference

- [`IHelpRenderer`](xref:AnyCli.Help.IHelpRenderer), [`DefaultHelpRenderer`](xref:AnyCli.Help.DefaultHelpRenderer)
- [`RootHelpContext`](xref:AnyCli.Help.RootHelpContext), [`CommandHelpContext`](xref:AnyCli.Help.CommandHelpContext)
- [`ApplicationInfo`](xref:AnyCli.Commands.Metadata.ApplicationInfo), [`CommandInfo`](xref:AnyCli.Commands.Metadata.CommandInfo), [`FrameworkOptionInfo`](xref:AnyCli.Commands.Metadata.FrameworkOptionInfo), [`FrameworkOptionKind`](xref:AnyCli.Commands.Metadata.FrameworkOptionKind)
- [`IHelpTextProvider`](xref:AnyCli.Help.IHelpTextProvider), [`LocalizedHelpTextProvider`](xref:AnyCli.Help.LocalizedHelpTextProvider)
- [`IHelpValueFormatter`](xref:AnyCli.Help.IHelpValueFormatter), [`DefaultHelpValueFormatter`](xref:AnyCli.Help.DefaultHelpValueFormatter)
- [`HelpRendererContext`](xref:AnyCli.Help.HelpRendererContext), [`HelpText`](xref:AnyCli.Help.HelpText), [`HelpTextResourceKeys`](xref:AnyCli.Help.HelpTextResourceKeys)
- [`HelpDescriptionAttribute`](xref:AnyCli.Help.HelpDescriptionAttribute), [`HelpDefaultValueAttribute`](xref:AnyCli.Help.HelpDefaultValueAttribute)
- [`HelpConfigurationBuilder`](xref:AnyCli.Commands.Registration.HelpConfigurationBuilder)
- [`IConsoleWriter`](xref:AnyCli.Console.IConsoleWriter), [`IOutputWriter`](xref:AnyCli.Console.IOutputWriter)

## Related

- [Customize help output](../guides/customize-help-output.md)
- [Localize help and errors](../guides/localize-help-and-errors.md)
- [Errors reference](errors.md)
- [Spectre.Console adapter reference](spectre-console-adapter.md)
- [Understanding help and error handling](../explanation/understanding-help-and-errors.md)
