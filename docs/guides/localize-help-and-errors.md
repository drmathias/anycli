<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Localize help and error messages

AnyCLI localizes built-in help headings, standard option descriptions, and command-line errors by default. Application-owned output, such as command results, is not translated automatically.

Use this guide when the application needs its own translated text, a language that AnyCLI does not ship, or a custom culture selection policy.

## Configure the cultures

By default, AnyCLI uses `CultureInfo.CurrentUICulture` for help and error text and `CultureInfo.CurrentCulture` for formatting values. These are the defaults from `LocalizationOptions.Default`.

Use `ConfigureLocalization` to choose a different UI culture and formatting culture:

```csharp
using System.Globalization;

using AnyCli.Localization;

var language = Environment.GetEnvironmentVariable("TODO_LANG");
var culture = string.IsNullOrWhiteSpace(language)
    ? CultureInfo.CurrentUICulture
    : CultureInfo.GetCultureInfo(language);

var app = ConsoleAppBuilder.Create()
    .WithName("todo")
    .ConfigureLocalization(LocalizationOptions.FromCulture(culture))
    // ...
    .Build();
```

Culture providers are evaluated at runtime, so a process can choose a culture after configuration:

```csharp
.ConfigureLocalization(new LocalizationOptions
{
    UICultureProvider = () => CultureInfo.CurrentUICulture,
    FormatCultureProvider = () => CultureInfo.CurrentCulture,
})
```

Use separate UI and format culture providers when help text should use one language while numbers, dates, and other values use a different locale.

## Create resource files

Use separate resource files for application text and framework overrides:

```text
HelpDescriptions.resx       application help descriptions and default value display
HelpOverrides.resx          overrides for built-in help text, if desired
ErrorOverrides.resx         overrides for built-in error messages, if desired
```

Keeping framework overrides separate from application text makes ownership clear and prevents application resource keys from accidentally replacing framework keys.

Add culture-specific files next to the neutral file:

```text
HelpDescriptions.de.resx
HelpOverrides.de.resx
ErrorOverrides.de.resx
```

Each entry maps a resource key to a value:

```xml
<data name="TitleDescription" xml:space="preserve">
  <value>Task title</value>
</data>
```

Use the key in `[HelpDescription(ResourceKey = "TitleDescription")]`. The neutral file is the fallback, and `.resx` files are embedded as resources by the SDK by default.

The examples use generated `ResourceManager` accessors such as `Resources.HelpDescriptions.ResourceManager` and `Resources.HelpOverrides.ResourceManager`. You can generate strongly typed access by adding the `Microsoft.CodeAnalysis.ResxSourceGenerator` package:

```xml
<PackageReference Include="Microsoft.CodeAnalysis.ResxSourceGenerator"
                  Version="..."
                  PrivateAssets="all" />
```

Without a source generator, create the resource manager manually using the file's base name and assembly:

```csharp
new ResourceManager(
    "TodoCli.HelpDescriptions",
    typeof(Program).Assembly)
```

The base name is normally the project root namespace plus the resource file name.

## Provide application help resources

AnyCLI provides localized help headings and standard option descriptions without extra configuration. `UseLocalizedTextProvider` makes application resource managers available to help, including the resources used by `[HelpDescription]` and `[HelpDefaultValue]`.

Register the application resources even when you are not overriding built-in text:

```csharp
.ConfigureHelp(help => help
    .UseLocalizedTextProvider(Resources.HelpDescriptions.ResourceManager))
```

Without this registration, resource keys such as `TitleDescription` cannot be resolved and the literal fallback text is used.

## Override built-in help text

Put replacements for built-in headings and standard help descriptions in `HelpOverrides.resx`. Keys come from `HelpTextResourceKeys`, such as `Usage`, `Options`, and `HelpOptionDescription`.

Register the override resources before the application resources so they take precedence:

```csharp
.ConfigureHelp(help => help
    .UseLocalizedTextProvider(
        Resources.HelpOverrides.ResourceManager,
        Resources.HelpDescriptions.ResourceManager))
```

Override files may contain only the keys being changed. Missing keys fall back to the built-in translations.

If the application uses Spectre.Console, `UseSpectreHelpRenderer` uses the same configured text provider:

```csharp
.ConfigureHelp(help => help
    .UseLocalizedTextProvider(
        Resources.HelpOverrides.ResourceManager,
        Resources.HelpDescriptions.ResourceManager)
    .UseSpectreHelpRenderer())
```

## Localize command and parameter descriptions

Command and group descriptions use `HelpText.FromResource`:

```csharp
.ConfigureCommands(commands => commands
    .AddCommand<ListCommand, ListParameters>(
        "list",
        HelpText.FromResource("ListDescription"))
    .AddCommand<AddCommand, AddParameters>(
        "add",
        HelpText.FromResource("AddDescription")))
```

Parameter descriptions use `[HelpDescription]` with a resource key and optional literal fallback:

```csharp
[Argument(0)]
[HelpDescription("Task title", ResourceKey = "TitleDescription")]
public required string Title { get; init; }

[Option("--priority", "-p")]
[HelpDescription("Task priority", ResourceKey = "PriorityDescription")]
public TodoPriority Priority { get; init; } = TodoPriority.Normal;
```

Default value display text uses `[HelpDefaultValue]`:

```csharp
[Option("--data")]
[HelpDefaultValue("tasks.json", ResourceKey = "DataFileDefault")]
public string DataFile { get; init; } = "tasks.json";
```

The literal text is used when the resource key is missing.

## Override built-in error messages

AnyCLI already provides localized command-line error messages. Put replacements for built-in messages in `ErrorOverrides.resx`:

```csharp
.ConfigureErrorHandling(errors => errors
    .UseLocalizedMessageProvider(Resources.ErrorOverrides.ResourceManager))
```

Keys correspond to `CommandErrorResourceKeys`, such as `UnknownCommand`, `UnknownOption`, or `MissingRequiredArgument`. Override resources are searched before the built-in messages.

For a custom message format rather than resource replacement, use a custom `ICommandErrorMessageProvider` and see [Customize command-line errors](customize-command-line-errors.md).

## Add another language to your app

Add a culture-specific `.resx` file next to the neutral resource file. Use a standard culture name such as `de`, `fr`, or `pt-BR`.

For application help descriptions:

```text
HelpDescriptions.resx          neutral fallback
HelpDescriptions.de.resx       German
HelpDescriptions.es.resx       Spanish
```

For framework help overrides:

```text
HelpOverrides.resx             neutral fallback
HelpOverrides.de.resx          German
HelpOverrides.es.resx          Spanish
```

For framework error overrides:

```text
ErrorOverrides.resx            neutral fallback
ErrorOverrides.de.resx         German
ErrorOverrides.es.resx         Spanish
```

No code changes are needed after the resource managers are registered. The resource manager selects the culture-specific file automatically. Keys missing from a culture-specific file fall back to the neutral file.

To fully localize a language that AnyCLI does not ship, also override the built-in keys from `HelpTextResourceKeys` and `CommandErrorResourceKeys` in `HelpOverrides.resx` and `ErrorOverrides.resx`. Otherwise those built-in strings fall back to English.

## Resource fallback

When a key is missing from the selected culture, the provider uses the neutral resource file. If the key is still missing, parameter help falls back to the literal `[HelpDescription]` text and framework messages fall back to their built-in text.

The built-in help and error resources include English, French, German, Spanish, Brazilian Portuguese, Italian, and Dutch. Unknown cultures fall back to English.

## Contribute translations to AnyCLI

AnyCLI accepts new translations for its built-in help and error text. A complete language adds both resource files:

```text
src/AnyCli/Help/HelpMessages.<culture>.resx
src/AnyCli/Errors/CommandErrorMessages.<culture>.resx
```

For example, an Italian contribution would use `it`:

```text
src/AnyCli/Help/HelpMessages.it.resx
src/AnyCli/Errors/CommandErrorMessages.it.resx
```

To contribute a translation:

1. Fork the [AnyCLI repository](https://github.com/drmathias/anycli) and create a branch.
2. Copy the neutral file `HelpMessages.resx` to `HelpMessages.<culture>.resx`.
3. Copy the neutral file `CommandErrorMessages.resx` to `CommandErrorMessages.<culture>.resx`.
4. Translate the `<value>` elements. Keep the `name` attributes unchanged.
5. Preserve format placeholders such as `{0}`, `{1}`, and `{2}`. You may reorder them to fit the language's grammar, but do not change their numbers.
6. Keep `xml:space="preserve"` on the data elements.
7. Add the culture to the expected translation tests in `tests/AnyCli.Tests/HelpLocalizationTests.cs` and `tests/AnyCli.Tests/ErrorHandlingTests.cs`.
8. Update the language lists in `README.md`, `docs/getting-started/localize-the-app.md`, and this guide.
9. Run `dotnet test` and `./scripts/lint.sh`.
10. Open a pull request with the translation and the updated tests.

Partial translations are allowed; missing keys fall back to the neutral English text. A complete file is preferred because it keeps the language support predictable.

Translations are intended for real users and not for machines. Therefore please only contribute translations if you are a fluent speaker in the language and not relying on translation technologies or LLMs.

## Localize application output

AnyCLI does not intercept text written by command bodies. Localize command results with the application's own resources or localization layer:

```csharp
context.Console.Output.WriteLine(Resources.Commands.ListEmpty);
```

Keep command output intended for scripts stable. Localize human-facing text rather than machine-readable values such as paths, JSON, or identifiers.

## Verify the result

Run the app with a specific culture:

```text
TODO_LANG=es dotnet run -- add --help
```

On Windows PowerShell:

```powershell
$env:TODO_LANG="es"; dotnet run -- add --help
```

Check help headings, command and parameter descriptions, default value display, and command-line errors. For automated checks, capture standard output and standard error separately; see [Test a CLI application](test-a-cli-application.md).

## Related

- [Localization tutorial](../getting-started/localize-the-app.md)
- [Help reference](../reference/help.md)
- [Errors reference](../reference/errors.md)
- [Customize help output](customize-help-output.md)
- [Customize command-line errors](customize-command-line-errors.md)
