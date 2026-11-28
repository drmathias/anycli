<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Write synchronous commands

Use synchronous commands for work that completes quickly without asynchronous I/O. Use asynchronous commands when the command needs to await I/O or observe cancellation.

## Typed parameters

Implement `ICommand<TParameters>` for a synchronous command with typed parameters:

```csharp
using AnyCli.Commands.Execution;

public sealed class GreetCommand : ICommand<GreetParameters>
{
    public int Execute(CommandContext context, GreetParameters parameters)
    {
        context.Console.Output.WriteLine($"Hello, {parameters.Name}!");
        return 0;
    }
}
```

Register it with `AddCommand`:

```csharp
.ConfigureCommands(commands => commands
    .AddCommand<GreetCommand, GreetParameters>("greet"))
```

The parameter type is a generated command parameters class:

```csharp
public sealed partial record GreetParameters
{
    [Argument(0)]
    public required string Name { get; init; }
}
```

## No parameters

Derive from `NoParametersCommand` when the command does not accept parameters:

```csharp
public sealed class CleanCommand : NoParametersCommand
{
    public override int Execute(CommandContext context)
    {
        context.Console.Output.WriteLine("Cleaned.");
        return 0;
    }
}
```

Register it with `AddCommand`:

```csharp
.ConfigureCommands(commands => commands.AddCommand<CleanCommand>("clean"))
```

`NoParametersCommand` implements the no-parameter synchronous command shape. Use `NoParametersAsyncCommand` for the asynchronous equivalent.

## Typed exit codes

Use `ICommand<TParameters, TExitCode>` for a synchronous command with typed parameters and an enum exit code:

```csharp
public sealed class CheckCommand : ICommand<CheckParameters, TodoExitCode>
{
    public TodoExitCode Execute(CommandContext context, CheckParameters parameters) =>
        parameters.Fail ? TodoExitCode.Failed : TodoExitCode.Success;
}
```

Register it through a typed-exit-code builder:

```csharp
.ConfigureCommands<TodoExitCode>(commands => commands
    .AddCommand<CheckCommand, CheckParameters>("check"))
```

Use `NoParametersCommand<TExitCode>` for a synchronous command without parameters that returns a typed exit code.

## Commands with subcommands

A command can both execute and own subcommands. Use the `AddCommand` overload with a child configuration callback:

```csharp
public sealed class ConfigCommand : NoParametersCommand
{
    public override int Execute(CommandContext context)
    {
        context.Console.Output.WriteLine("Use a config subcommand.");
        return 0;
    }
}

public sealed class ConfigShowCommand : NoParametersCommand
{
    public override int Execute(CommandContext context)
    {
        context.Console.Output.WriteLine("Showing configuration.");
        return 0;
    }
}
```

```csharp
.ConfigureCommands(commands => commands
    .AddCommand<ConfigCommand>(
        "config",
        "Manage configuration",
        config => config.AddCommand<ConfigShowCommand>("show", "Show configuration")))
```

`todo config` runs `ConfigCommand`; `todo config show` runs the child command. Command groups created with `AddGroup` are containers only and do not execute.

The asynchronous equivalents use `IAsyncCommand`, `NoParametersAsyncCommand`, `AddAsyncCommand`, and the same child configuration callback.

## When to choose synchronous commands

Use synchronous commands when:

- the work is CPU-bound or completes quickly
- there is no asynchronous API to await
- cancellation is not needed inside the command body

Use asynchronous commands when the command performs file, network, or other I/O, or when it should observe the cancellation token passed by `RunAsync`.

## Related

- [Command-line syntax reference](../reference/command-line-syntax.md)
- [Arguments and options tutorial](../getting-started/arguments-and-options.md)
- [Command groups tutorial](../getting-started/command-groups.md)
- [Handle cancellation](handle-cancellation.md)
