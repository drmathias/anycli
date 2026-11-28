<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Customize help output

Help customization has three levels:

- change the text and localization
- change how default values are formatted
- replace the help layout itself

Choose the smallest level that solves the problem.

## Change help text and localization

Parameter descriptions use `[HelpDescription]`:

```csharp
[Option("--data")]
[HelpDescription("Task data file", ResourceKey = "DataFileDescription")]
public string DataFile { get; init; } = "";
```

Use `UseLocalizedTextProvider` when descriptions come from resource files:

```csharp
var app = ConsoleAppBuilder.Create()
    .WithName("todo")
    .ConfigureHelp(help => help
        .UseLocalizedTextProvider(Resources.HelpDescriptions.ResourceManager))
    .ConfigureCommands(commands => commands
        .AddCommand<ListCommand, ListParameters>("list"))
    .Build();
```

To replace headings, labels, placeholders, and description resolution entirely, implement `IHelpTextProvider` and register it by type or instance:

```csharp
.ConfigureHelp(help => help.UseTextProvider<MyHelpTextProvider>())
// or
.ConfigureHelp(help => help.UseTextProvider(new MyHelpTextProvider(options)))
```

## Change default value formatting

Use `[HelpDefaultValue]` when a parameter needs explicit display text:

```csharp
[Option("--data")]
[HelpDefaultValue("$HOME/.config/todo/tasks.json")]
public string DataFile { get; init; } = "";
```

`[HelpDefaultValue]` affects help only. It does not change binding or invoke a default value provider.

To change formatting for all fallback defaults, implement `IHelpValueFormatter` and register it by type or instance:

```csharp
.ConfigureHelp(help => help.UseValueFormatter<MyHelpValueFormatter>())
// or
.ConfigureHelp(help => help.UseValueFormatter(new MyHelpValueFormatter(options)))
```

## Replace the help layout

Implement `IHelpRenderer` when headings, sections, spacing, or ordering are not enough:

```csharp
public sealed class MyHelpRenderer : IHelpRenderer
{
    public ValueTask WriteRootHelpAsync(
        RootHelpContext context,
        CancellationToken cancellationToken)
    {
        context.Console.Output.WriteLine(context.Application.AppName);
        return ValueTask.CompletedTask;
    }

    public ValueTask WriteCommandHelpAsync(
        CommandHelpContext context,
        CancellationToken cancellationToken)
    {
        context.Console.Output.WriteLine(context.Command.Name);
        return ValueTask.CompletedTask;
    }
}
```

Register it with:

```csharp
.ConfigureHelp(help => help.UseRenderer<MyHelpRenderer>())
```

A renderer can read:

- `context.Application` for the app name, global options, framework options, and root command tree
- `context.Command` in command help for the selected command or group
- `context.Console.Output` for help output

If the renderer needs services, register it in the application service provider. AnyCLI resolves it from the service provider first and falls back to its public parameterless constructor.

Type-based text, formatter, and renderer registrations use DI or a parameterless constructor. Instance-based registrations reuse the supplied object across runs. The localized-resource overloads create a text provider using the configured localization options.

## Use Spectre.Console styling

For styled help based on the shared help metadata and text, use the Spectre.Console adapter:

```csharp
.ConfigureHelp(help => help.UseSpectreHelpRenderer())
```

See the [Spectre.Console adapter reference](../reference/spectre-console-adapter.md) and the [styled output tutorial](../getting-started/styled-output-with-spectreconsole.md).

## Test the result

Run the app with `--help` for root help and `<command> --help` for command help. For automated tests, compare captured standard output; see [Test a CLI application](test-a-cli-application.md).

## Related

- [Help reference](../reference/help.md)
- [Set default values](default-values.md)
- [Understanding help and error handling](../explanation/understanding-help-and-errors.md)
