<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Handle cancellation

Async commands and interceptors receive a `CancellationToken`. Use it to stop work when the user presses `Ctrl+C` or when another part of the application requests cancellation.

## Wire Ctrl+C to a token

Create a `CancellationTokenSource`, cancel it from `Console.CancelKeyPress`, and pass its token to `RunAsync`:

```csharp
using var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    return await app.RunAsync(args, cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("Operation cancelled.");
    return 130;
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

Setting `eventArgs.Cancel = true` prevents the runtime from terminating immediately, so commands can clean up before the process exits. Exit code `130` is the conventional shell code for `Ctrl+C`.

`RunAsync` checks the token before dispatch and passes it through interceptors to the command. A cancelled token therefore prevents a command from starting and interrupts one that is already running.

## Observe cancellation in commands

Pass the token to cancellable operations and check it between units of work:

```csharp
public sealed class ImportCommand
    : IAsyncCommand<ImportParameters, TodoExitCode>
{
    public async Task<TodoExitCode> ExecuteAsync(
        CommandContext context,
        ImportParameters parameters,
        CancellationToken cancellationToken)
    {
        await foreach (var line in File.ReadLinesAsync(
            parameters.Source,
            cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ImportLineAsync(line, cancellationToken);
        }

        return TodoExitCode.Success;
    }

    private static Task ImportLineAsync(
        string line,
        CancellationToken cancellationToken) =>
        Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
}
```

Cancellation is cooperative: a command keeps running until it observes the token. Use `ThrowIfCancellationRequested` for synchronous checks and pass the token to async APIs such as file, network, and input operations:

```csharp
var input = await context.Console.Input.ReadLineAsync(cancellationToken);
```

Do not catch `OperationCanceledException` and return success. Let it propagate unless you need to perform cleanup first; in that case, rethrow it after cleanup.

## Cancel from interceptors

Interceptors receive the same token and can pass it to the next stage:

```csharp
using AnyCli.Interceptors;

public sealed class TimeoutInterceptor
    : ICommandInterceptor
{
    public async Task<int> InterceptAsync(
        CommandInterceptorContext context,
        Func<CancellationToken, Task<int>> next,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken);

        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        return await next.Invoke(timeout.Token);
    }
}
```

A linked token source lets an interceptor add its own cancellation condition, such as a timeout, without discarding cancellation from `Ctrl+C`.

## Related

- [Write an interceptor](write-an-interceptor.md)
- [Typed exit codes](../getting-started/typed-exit-codes.md)
- [Logging with interceptors](../getting-started/logging-with-interceptors.md)
