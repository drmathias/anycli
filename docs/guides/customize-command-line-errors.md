<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Customize command-line errors

Command-line errors are separate from exceptions thrown by command bodies. This guide covers changing how parsing errors are worded, rendered, handled, and returned as exit codes.

## Change error messages

The default message provider is localized. Put replacements for built-in messages in a separate `ErrorOverrides.resx` file and register it with `UseLocalizedMessageProvider`:

```csharp
var app = ConsoleAppBuilder.Create()
    .WithName("todo")
    .ConfigureErrorHandling(errors => errors
        .UseLocalizedMessageProvider(Resources.ErrorOverrides.ResourceManager))
    .ConfigureCommands(commands => commands
        .AddCommand<RunCommand, RunParameters>("run"))
    .Build();
```

To replace message generation entirely, implement `ICommandErrorMessageProvider`:

```csharp
public sealed class MyErrorMessageProvider : ICommandErrorMessageProvider
{
    public string GetMessage(CommandErrorContext context) =>
        context.Error switch
        {
            UnknownOptionError error => $"Unknown option: {error.OptionName}",
            _ => "Invalid command-line input.",
        };
}
```

Register it by type or instance:

```csharp
.ConfigureErrorHandling(errors => errors
    .UseMessageProvider<MyErrorMessageProvider>())
// or
.ConfigureErrorHandling(errors => errors
    .UseMessageProvider(new MyErrorMessageProvider(options)))
```

## Change error presentation

Implement `ICommandErrorWriter` to change styling, prefixes, or output formatting:

```csharp
public sealed class MyErrorWriter : ICommandErrorWriter
{
    public ValueTask WriteAsync(
        CommandErrorContext context,
        string message,
        CancellationToken cancellationToken)
    {
        context.Console.Error.WriteLine($"error: {message}");
        return ValueTask.CompletedTask;
    }
}
```

Register it by type or instance:

```csharp
.ConfigureErrorHandling(errors => errors.UseWriter<MyErrorWriter>())
// or
.ConfigureErrorHandling(errors => errors.UseWriter(new MyErrorWriter(options)))
```

For styled errors, use the Spectre.Console adapter:

```csharp
.ConfigureErrorHandling(errors => errors.UseSpectreErrorWriter())
```

See the [Spectre.Console adapter reference](../reference/spectre-console-adapter.md).

## Replace error handling

Implement `ICommandErrorHandler` when the application needs to do more than write a message, such as logging, returning structured output, or applying custom routing:

```csharp
public sealed class MyErrorHandler : ICommandErrorHandler
{
    public ValueTask HandleAsync(
        CommandErrorContext context,
        CancellationToken cancellationToken)
    {
        context.Console.Error.WriteLine(context.Error);
        return ValueTask.CompletedTask;
    }
}
```

Register it by type:

```csharp
.ConfigureErrorHandling(errors => errors.UseHandler<MyErrorHandler>())
```

To wrap the configured message provider and writer, use the handler factory:

```csharp
.ConfigureErrorHandling(errors => errors
    .UseHandler(context => new LoggingErrorHandler(
        context.DefaultHandler,
        logger)))
```

The handler factory receives `CommandErrorHandlerContext`, which exposes:

- `ServiceProvider`
- `MessageProvider`
- `Writer`
- `Localization`
- `DefaultHandler`

The runtime error context passed to `HandleAsync` exposes `Application`, `CommandPath`, `Error`, and `Console`.

## Change the exit code

Command-line errors return `2` by default. Set a fixed value:

```csharp
.ConfigureErrorHandling(errors => errors.UseExitCode(64))
```

Use a typed enum value:

```csharp
.ConfigureErrorHandling(errors => errors.UseExitCode(TodoExitCode.CommandLineError))
```

Derive a value from the error:

```csharp
.ConfigureErrorHandling(errors => errors.UseExitCode(error =>
    error is InvalidParameterValueError ? 65 : 2))
```

You can also return a typed exit-code enum:

```csharp
.ConfigureErrorHandling(errors => errors.UseExitCode(error =>
    error is InvalidParameterValueError
        ? TodoExitCode.InvalidInput
        : TodoExitCode.CommandLineError))
```

## Scope

This configuration applies to command-line parsing errors. Exceptions thrown by command bodies are not command-line errors and are not converted by this pipeline. Cancellation is also separate.

Type-based registrations use DI or a parameterless constructor. Instance-based registrations reuse the supplied object across runs. Use an instance overload when construction needs configuration that is not registered in DI.

## Related

- [Errors reference](../reference/errors.md)
- [Localize help and errors](localize-help-and-errors.md)
- [Help reference](../reference/help.md)
- [Spectre.Console adapter reference](../reference/spectre-console-adapter.md)
- [Understanding help and error handling](../explanation/understanding-help-and-errors.md)
