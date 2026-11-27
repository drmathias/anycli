<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Dependency Injection

As a CLI app grows, it becomes important to decouple components in your code, to aid maintainence and testing of the application. `todo` currently creates a `JsonTodoStore` inside each command, which couples commands to JSON persistence and makes them harder to test in isolation. In this step, you'll introduce `ITodoStore`, inject it into the commands, and compose the application with Microsoft.Extensions.DependencyInjection.

AnyCLI only requires an `IServiceProvider`, so the same approach works with other DI containers.

## Add the package

Add Microsoft.Extensions.DependencyInjection:

```bash
dotnet add package Microsoft.Extensions.DependencyInjection
```

## Extract the store interface

Introduce `ITodoStore` and rename `TodoStore` to `JsonTodoStore`:

```csharp
public interface ITodoStore
{
    Task<IReadOnlyList<TodoItem>> LoadAsync(CancellationToken cancellationToken);

    Task SaveAsync(
        IReadOnlyList<TodoItem> tasks,
        CancellationToken cancellationToken);
}

public sealed class JsonTodoStore(string path) : ITodoStore
{
    public static string DataPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TodoCli",
        "todo.json");

    // Existing LoadAsync and SaveAsync methods.
}
```

The existing `LoadAsync` and `SaveAsync` methods already match the interface.

## Inject the store into the commands

Change the command constructors to accept `ITodoStore`:

```csharp
public sealed class ListCommand(ITodoStore store) : NoParametersAsyncCommand<TodoExitCode>
{
    // Existing command body.
}

public sealed class AddCommand(ITodoStore store) : IAsyncCommand<AddParameters, TodoExitCode>
{
    // Existing command body.
}

public sealed class DoneCommand(ITodoStore store) : IAsyncCommand<DoneParameters, TodoExitCode>
{
    // Existing command body.
}
```

Remove the `new JsonTodoStore(JsonTodoStore.DataPath)` lines from the command bodies and use the injected `store` instead:

```csharp
var tasks = (await store.LoadAsync(cancellationToken)).ToList();
await store.SaveAsync(tasks, cancellationToken);
```

`ListCommand` should call `store.LoadAsync` without converting the result to a list.

## Configure the container

Register the store and the commands, then pass the provider to `UseServiceProvider`:

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<ITodoStore>(new JsonTodoStore(JsonTodoStore.DataPath));
services.AddTransient<ListCommand>();
services.AddTransient<AddCommand>();
services.AddTransient<DoneCommand>();

using var provider = services.BuildServiceProvider();

var app = ConsoleAppBuilder.Create()
    .UseServiceProvider(provider)
    .WithName("todo")
    .ConfigureHelp(help => help
        .UseSpectreHelp())
    .ConfigureErrorHandling(errors => errors
        .UseSpectreErrors()
        .UseExitCode(TodoExitCode.CommandLineError))
    .ConfigureCommands<TodoExitCode>(commands => commands
        .AddAsyncCommand<ListCommand>("list", "List all tasks")
        .AddAsyncCommand<AddCommand, AddParameters>("add", "Add a task")
        .AddAsyncCommand<DoneCommand, DoneParameters>("done", "Mark a task as done"))
    .Build();
```

The store is a singleton because it holds no per-run state. Commands are transient because AnyCLI resolves a fresh command instance for each run.

Registration is only required for commands, interceptors, providers, handlers, and writers that need constructor dependencies. Types with a public parameterless constructor can be created without registration.

## Run the app

Behaviour is unchanged:

```bash
dotnet run -- add "Write docs" --priority high
dotnet run -- list
```

## Test with a fake store

The app now depends on `ITodoStore`, so a test can replace JSON persistence with an in-memory implementation or a mock.

## Learn more

- [Test a CLI application](../guides/test-a-cli-application.md)

## Next

- [Logging With Interceptors](logging-with-interceptors.md)
