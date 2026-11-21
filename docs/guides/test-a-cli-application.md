<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Test a CLI application

It is preferrable to test the whole `ConsoleApp`, not just the command class. That exercises parsing, dependency injection, help and error handling, and the process exit code.

The examples use TUnit. Replace the attributes and assertions with your test framework's equivalents if needed.

## Create a testable app factory

This example builds on the app from [Dependency Injection](../getting-started/dependency-injection.md). Extract the application composition into a factory that accepts the store as a dependency:

```csharp
public static class TodoApp
{
    public static ConsoleApp Create(ITodoStore store)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITodoStore>(store);
        services.AddTransient<ListCommand>();
        services.AddTransient<AddCommand>();
        services.AddTransient<DoneCommand>();

        var provider = services.BuildServiceProvider();

        return ConsoleAppBuilder.Create()
            .UseServiceProvider(provider)
            .WithName("todo")
            .ConfigureHelp(help => help
                .UseSpectreHelp())
            .ConfigureErrorHandling(errors => errors
                .UseSpectreErrors()
                .UseExitCode(TodoExitCode.CommandLineError))
            .ConfigureCommands<TodoExitCode>(commands => commands
                .AddAsyncCommand<ListCommand, ListParameters>("list", "List all tasks")
                .AddAsyncCommand<AddCommand, AddParameters>("add", "Add a task")
                .AddAsyncCommand<DoneCommand, DoneParameters>("done", "Mark a task as done"))
            .Build();
    }
}
```

Production passes the JSON store for the current user; tests pass a fake:

```csharp
var app = TodoApp.Create(new JsonTodoStore(JsonTodoStore.DataPath));
```

```csharp
var store = new InMemoryTodoStore();
var app = TodoApp.Create(store);
```

Add other dependencies as parameters as the application grows.

## Capture console I/O

`ConsoleApp.RunAsync` accepts an `IConsole`. Use `TextConsole` with `StringWriter` instances to capture output and error separately:

```csharp
using AnyCli.Commands.Execution;
using AnyCli.Console;

public sealed record AppResult(int ExitCode, string Output, string Error);

public static class AppRunner
{
    public static async Task<AppResult> RunAsync(
        ConsoleApp app,
        params string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(TextReader.Null, output, error));

        return new AppResult(
            exitCode,
            output.ToString(),
            error.ToString());
    }
}
```

## Test exit codes and errors

```csharp
[Test]
public async Task Done_WithUnknownId_ReturnsTaskNotFound()
{
    var app = TodoApp.Create(new InMemoryTodoStore());

    var result = await AppRunner.RunAsync(app, "done", "99");

    await Assert.That(result.ExitCode)
        .IsEqualTo((int)TodoExitCode.TaskNotFound);
    await Assert.That(result.Error)
        .Contains("Task 99 was not found.");
}
```

Use the same pattern for command-line errors:

```csharp
[Test]
public async Task Add_WithoutTitle_ReturnsCommandLineError()
{
    var app = TodoApp.Create(new InMemoryTodoStore());

    var result = await AppRunner.RunAsync(app, "add");

    await Assert.That(result.ExitCode)
        .IsEqualTo((int)TodoExitCode.CommandLineError);
    await Assert.That(result.Error).IsNotEmpty();
}
```

## Inject a fake service

Use the `InMemoryTodoStore` from [Dependency Injection](../getting-started/dependency-injection.md), or another fake that records saved tasks. Inspect it through its interface after the command runs:

```csharp
[Test]
public async Task Add_PersistsTask()
{
    var store = new InMemoryTodoStore();
    var app = TodoApp.Create(store);

    var result = await AppRunner.RunAsync(
        app,
        "add", "Write docs", "--priority", "high");

    var saved = await store.LoadAsync(TestContext.Current.CancellationToken);

    await Assert.That(result.ExitCode).IsEqualTo(0);
    await Assert.That(saved.Count).IsEqualTo(1);
    await Assert.That(saved[0].Title).IsEqualTo("Write docs");
}
```

## Test input

For commands that read input, pass a `StringReader` as the `TextConsole` input instead of `TextReader.Null`.

## Test behavior

- Create a fresh app and fake services for each test.
- Keep the factory as the single composition point; vary dependencies and console streams instead of changing the app composition in tests.
- Assert exit codes as well as output.
- Prefer assertions on visible text over exact ANSI sequences.
- For deterministic ANSI assertions, test Spectre output through `SpectreConsoleAdapter` with `SpectreConsoleOptions.ForTesting`.
- Test interceptors through the application so their ordering and short-circuit behavior are exercised.

## Related

- [Dependency Injection](../getting-started/dependency-injection.md)
- [Spectre.Console adapter reference](../reference/spectre-console-adapter.md)
