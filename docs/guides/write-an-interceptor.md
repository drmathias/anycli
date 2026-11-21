<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Write an interceptor

Interceptors wrap command execution. Use them for cross-cutting behavior such as logging, timing, validation, setup, and telemetry.

## Implement an interceptor

Implement `ICommandInterceptor` and call `next` to continue the pipeline:

```csharp
using System.Diagnostics;

using AnyCli.Interceptors;
using Microsoft.Extensions.Logging;

public sealed class TimingInterceptor(
    ILogger<TimingInterceptor> logger) : ICommandInterceptor
{
    public async Task<int> InterceptAsync(
        CommandInterceptorContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var exitCode = await next(cancellationToken);
        stopwatch.Stop();

        logger.LogInformation(
            "{Command} exited with {ExitCode} in {ElapsedMilliseconds}ms",
            context.RawCommand,
            exitCode,
            stopwatch.ElapsedMilliseconds);

        return exitCode;
    }
}
```

The code before `next` runs before the command. The code after `next` runs after the command returns.

## Register the interceptor

Register interceptors on the application builder in the order they should run. See [execution order](../reference/interceptors.md#execution-order).

```csharp
services.AddTransient<TimingInterceptor>();

var app = ConsoleAppBuilder.Create()
    .UseServiceProvider(provider)
    .WithName("todo")
    .AddGlobalOptions<TodoGlobalOptions>()
    .ConfigureCommands<TodoExitCode>(commands => commands
        .AddAsyncCommand<ListCommand, ListParameters>("list", "List all tasks"))
    .AddInterceptor<TimingInterceptor>()
    .Build();
```

## Read the command context

`CommandInterceptorContext` exposes `RawCommand`, `ParsedArguments`, and `ParsedOptions`, plus the inherited `Name`, `CommandPath`, and `Console`. See [interceptor context](../reference/interceptors.md#interceptor-context) and [parsed value shapes](../reference/interceptors.md#parsed-value-shapes) for the full contract.

Use the parsed dictionaries for cross-cutting inspection:

```csharp
var title = context.ParsedArguments.TryGetValue("title", out var titleParameter)
    && titleParameter is ParsedValue.Scalar { Value: string titleValue }
        ? titleValue
        : "";
var priority = context.ParsedOptions.TryGetValue("--priority", out var priorityParameter)
    && priorityParameter is ParsedValue.Scalar { Value: string priorityValue }
        ? priorityValue
        : "";
```

## Short-circuit execution

Return without calling `next` to prevent the command from running. A read-only mode guard allows queries but blocks commands that mutate state:

```csharp
public sealed class ReadOnlyInterceptor(
    IApplicationMode mode) : ICommandInterceptor
{
    private static readonly HashSet<string> MutatingCommands = ["add", "done"];

    public async Task<int> InterceptAsync(
        CommandInterceptorContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken)
    {
        if (!mode.IsReadOnly || !MutatingCommands.Contains(context.Name))
        {
            return await next(cancellationToken);
        }

        context.Console.Error.WriteLine("The todo list is read-only.");
        return (int)TodoExitCode.CommandLineError;
    }
}
```

See [scope](../reference/interceptors.md#scope) for when interceptors run.

## Work with typed global options

Use `ICommandInterceptor<TGlobalOptions>` to receive `CommandInterceptorContext<TGlobalOptions>`:

```csharp
public sealed class AuditInterceptor(
    IAuditLog auditLog) : ICommandInterceptor<TodoGlobalOptions>
{
    public Task<int> InterceptAsync(
        CommandInterceptorContext<TodoGlobalOptions> context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken)
    {
        auditLog.SetLevel(context.GlobalOptions.LogLevel);
        return next(cancellationToken);
    }
}
```

The application must be configured with `.AddGlobalOptions<TodoGlobalOptions>()` for the typed context to be available.

## Work with typed exit codes

Use `ICommandInterceptorWithExitCode<TExitCode>` when commands return an enum exit code:

```csharp
public sealed class ValidationInterceptor
    : ICommandInterceptorWithExitCode<TodoExitCode>
{
    public async Task<TodoExitCode> InterceptAsync(
        CommandInterceptorContext context,
        Func<CancellationToken, Task<TodoExitCode>> next,
        CancellationToken cancellationToken)
    {
        if (context.Name == "add" &&
            context.ParsedArguments.TryGetValue("title", out var value) &&
            value is ParsedValue.Scalar { Value: string title } &&
            title.Length > 50)
        {
            context.Console.Error.WriteLine(
                "Task titles must be 50 characters or fewer.");
            return TodoExitCode.CommandLineError;
        }

        return await next(cancellationToken);
    }
}
```

## Work with typed global options and exit codes

Use `ICommandInterceptorWithExitCode<TGlobalOptions, TExitCode>` when the interceptor needs both:

```csharp
public sealed class GlobalAuditInterceptor
    : ICommandInterceptorWithExitCode<TodoGlobalOptions, TodoExitCode>
{
    public async Task<TodoExitCode> InterceptAsync(
        CommandInterceptorContext<TodoGlobalOptions> context,
        Func<CancellationToken, Task<TodoExitCode>> next,
        CancellationToken cancellationToken)
    {
        context.Console.Output.WriteLine(
            $"Audit: {context.Name} with log level {context.GlobalOptions.LogLevel}");

        var exitCode = await next(cancellationToken);

        context.Console.Output.WriteLine(
            $"Audit: {context.Name} exited with {exitCode}");

        return exitCode;
    }
}
```

The application must configure both global options and typed commands, such as with `.AddGlobalOptions<TodoGlobalOptions>()` and `.ConfigureCommands<TodoExitCode>(...)`.

## Related

- [Interceptors reference](../reference/interceptors.md)
- [Logging With Interceptors](../getting-started/logging-with-interceptors.md)
- [Handle Cancellation](handle-cancellation.md)
- [Test a CLI application](test-a-cli-application.md)
