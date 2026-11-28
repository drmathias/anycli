<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Localize the App

AnyCLI localizes its built-in help headings, standard option descriptions, and command-line errors for English, French, German, Spanish, Brazilian Portuguese, Italian, and Dutch. In this step, you'll choose the UI culture at runtime and localize the text that belongs to `todo`.

## Configure the culture

Add the remaining usings to `Program.cs`:

```csharp
using System.Globalization;
using System.Resources;

using AnyCli.Help;
using AnyCli.Localization;
```

Read the requested language from an environment variable, falling back to the current UI culture, then configure localization on the application builder:

```csharp
var language = Environment.GetEnvironmentVariable("TODO_LANG");
var culture = string.IsNullOrWhiteSpace(language)
    ? CultureInfo.CurrentUICulture
    : CultureInfo.GetCultureInfo(language);

var localization = LocalizationOptions.FromCulture(culture);
TodoText.Configure(localization);

var app = ConsoleAppBuilder.Create()
    .UseServiceProvider(provider)
    .WithName("todo")
    .AddGlobalOptions<TodoGlobalOptions>()
    .ConfigureLocalization(localization)
    .ConfigureHelp(help => help
        .UseLocalizedTextProvider(
            new ResourceManager(
                "TodoCli.HelpDescriptions",
                typeof(JsonTodoStore).Assembly))
        .UseSpectreHelpRenderer())
    .ConfigureErrorHandling(errors => errors
        .UseSpectreErrorWriter()
        .UseExitCode(TodoExitCode.CommandLineError))
    .ConfigureCommands<TodoExitCode>(commands => commands
        .AddAsyncCommand<ListCommand, ListParameters>(
            "list", HelpText.FromResource("ListDescription"))
        .AddAsyncCommand<AddCommand, AddParameters>(
            "add", HelpText.FromResource("AddDescription"))
        .AddAsyncCommand<DoneCommand, DoneParameters>(
            "done", HelpText.FromResource("DoneDescription"))
        .AddGroup("data", HelpText.FromResource("DataDescription"), data => data
            .AddAsyncCommand<DataPathCommand, DataPathParameters>(
                "path", HelpText.FromResource("DataPathDescription"))
            .AddAsyncCommand<DataResetCommand, DataResetParameters>(
                "reset", HelpText.FromResource("DataResetDescription"))))
    .AddInterceptor<LogLevelInterceptor>()
    .AddInterceptor<LoggingInterceptor>()
    .Build();
```

`LocalizationOptions.FromCulture` uses the same culture for both UI text and formatting. If you need different cultures, configure `UICultureProvider` and `FormatCultureProvider` separately.

## Add the resources

Create `HelpDescriptions.resx` in the project root for the fallback text. The neutral file contains the application help text and command output text:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="ListDescription" xml:space="preserve">
    <value>List all tasks</value>
  </data>
  <data name="AddDescription" xml:space="preserve">
    <value>Add a task</value>
  </data>
  <data name="DoneDescription" xml:space="preserve">
    <value>Mark a task as done</value>
  </data>
  <data name="DataDescription" xml:space="preserve">
    <value>Manage task data</value>
  </data>
  <data name="DataPathDescription" xml:space="preserve">
    <value>Show the task data path</value>
  </data>
  <data name="DataResetDescription" xml:space="preserve">
    <value>Clear all tasks</value>
  </data>
  <data name="TitleDescription" xml:space="preserve">
    <value>Task title</value>
  </data>
  <data name="PriorityDescription" xml:space="preserve">
    <value>Task priority</value>
  </data>
  <data name="LogLevelDescription" xml:space="preserve">
    <value>Minimum log level</value>
  </data>
  <data name="NoTasksYet" xml:space="preserve">
    <value>No tasks yet.</value>
  </data>
  <data name="AddFirstTaskHint" xml:space="preserve">
    <value>Add one with: todo add "Write docs"</value>
  </data>
  <data name="TaskAdded" xml:space="preserve">
    <value>Added task {0}: {1} ({2})</value>
  </data>
  <data name="TaskNotFound" xml:space="preserve">
    <value>Task {0} was not found.</value>
  </data>
  <data name="TasksSummary" xml:space="preserve">
    <value>{0} tasks · {1} done · {2} pending</value>
  </data>
</root>
```

The Spanish file `HelpDescriptions.es.resx` uses the same keys:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="ListDescription" xml:space="preserve">
    <value>Listar todas las tareas</value>
  </data>
  <data name="AddDescription" xml:space="preserve">
    <value>Añadir una tarea</value>
  </data>
  <data name="DoneDescription" xml:space="preserve">
    <value>Marcar una tarea como completada</value>
  </data>
  <data name="DataDescription" xml:space="preserve">
    <value>Gestionar los datos de las tareas</value>
  </data>
  <data name="DataPathDescription" xml:space="preserve">
    <value>Mostrar la ruta de los datos</value>
  </data>
  <data name="DataResetDescription" xml:space="preserve">
    <value>Borrar todas las tareas</value>
  </data>
  <data name="TitleDescription" xml:space="preserve">
    <value>Título de la tarea</value>
  </data>
  <data name="PriorityDescription" xml:space="preserve">
    <value>Prioridad de la tarea</value>
  </data>
  <data name="LogLevelDescription" xml:space="preserve">
    <value>Nivel mínimo de registro</value>
  </data>
  <data name="NoTasksYet" xml:space="preserve">
    <value>Aún no hay tareas.</value>
  </data>
  <data name="AddFirstTaskHint" xml:space="preserve">
    <value>Añade una con: todo add "Escribir documentación"</value>
  </data>
  <data name="TaskAdded" xml:space="preserve">
    <value>Tarea {0} añadida: {1} ({2})</value>
  </data>
  <data name="TaskNotFound" xml:space="preserve">
    <value>No se encontró la tarea {0}.</value>
  </data>
  <data name="TasksSummary" xml:space="preserve">
    <value>{0} tareas · {1} completadas · {2} pendientes</value>
  </data>
</root>
```

The standard `.resx` schema and designer metadata are omitted from these examples for brevity; Visual Studio or the resource editor can generate them.

The neutral resource file is the fallback when a key is missing from a translated file. The SDK embeds `.resx` files as resources automatically. `TodoCli.HelpDescriptions` is the project root namespace plus the resource file name; if your project uses a different root namespace, use that instead.

For strongly typed resource access, add the `Microsoft.CodeAnalysis.ResxSourceGenerator` package and use its generated accessor. This tutorial uses the resource base name directly so the generated type is optional. See [Create resource files](../guides/localize-help-and-errors.md#create-resource-files) for the generator setup.

If you later replace built-in help or error text, keep those keys in separate `HelpOverrides.resx` and `ErrorOverrides.resx` files. That keeps application text distinct from framework overrides.

## Localize parameter descriptions

Combine the literal fallback text and a resource key in `[HelpDescription]`. Update `AddParameters` and `TodoGlobalOptions`:

```csharp
public sealed partial record AddParameters : TodoGlobalOptions
{
    [Argument(0)]
    [HelpDescription("Task title", ResourceKey = "TitleDescription")]
    public required string Title { get; init; }

    [Option("--priority", "-p")]
    [HelpDescription("Task priority", ResourceKey = "PriorityDescription")]
    public TodoPriority Priority { get; init; } = TodoPriority.Normal;
}

[GlobalOptions]
public partial record TodoGlobalOptions
{
    [Option("--log-level", "-l")]
    [HelpDescription("Minimum log level", ResourceKey = "LogLevelDescription")]
    public LogLevel LogLevel { get; init; } = LogLevel.Information;
}
```

Command descriptions use `HelpText.FromResource(...)` in the registration above. Parameter descriptions use `[HelpDescription]` because they are declared on the parameter properties.

## Localize console output

AnyCLI localizes help and built-in command-line errors, but it does not translate text written by command bodies. Use the same resource file for human-facing command output.

Add a small helper that uses the configured UI and formatting cultures:

```csharp
public static class TodoText
{
    private static readonly ResourceManager Resources = new(
        "TodoCli.HelpDescriptions",
        typeof(TodoItem).Assembly);

    private static LocalizationOptions _localization = LocalizationOptions.Default;

    public static void Configure(LocalizationOptions localization) =>
        _localization = localization;

    public static string Get(string key) =>
        Resources.GetString(key, _localization.GetUICulture()) ?? key;

    public static string Format(string key, params object?[] args) =>
        string.Format(_localization.GetFormatCulture(), Get(key), args);
}
```

`TodoText.Configure(localization)` is called when the application starts. The helper uses the same culture providers as help and error handling.

Update the empty-list output in `ListCommand`:

```csharp
context.AnsiConsole.Output.MarkupLineInterpolated(
    $"[yellow]{TodoText.Get("NoTasksYet")}[/]");
context.AnsiConsole.Output.MarkupLineInterpolated(
    $"[grey]{TodoText.Get("AddFirstTaskHint")}[/]");
```

Update the success message in `AddCommand`:

```csharp
var message = TodoText.Format(
    "TaskAdded",
    id,
    Markup.Escape(parameters.Title),
    parameters.Priority.ToString().ToLowerInvariant());

context.AnsiConsole.Output.MarkupLineInterpolated($"[green]{message}[/]");
```

Use the same approach for the task-not-found message in `DoneCommand`:

```csharp
context.AnsiConsole.Error.MarkupLineInterpolated(
    $"[red]{TodoText.Format("TaskNotFound", parameters.Id)}[/]");
```

Apply the same pattern to table headers, status labels, priority labels, and summary text. Keep machine-readable output such as JSON and file paths stable.

## Run the app

On macOS or Linux, select Spanish for one invocation:

```text
TODO_LANG=es dotnet run -- add --help
```

On Windows PowerShell:

```powershell
$env:TODO_LANG="es"; dotnet run -- add --help
```

![Spanish help output for the add command](../images/todo-help-es.png)

The command description, argument description, option description, global option description, built-in help text, and application command output all come from the selected culture. Built-in command-line errors are localized in the same way.

## Learn more

- [Localize help and error messages](../guides/localize-help-and-errors.md)
- [Help reference](../reference/help.md)
- [Errors reference](../reference/errors.md)

## Next

- [Publish and Distribute](publish-and-distribute.md)
