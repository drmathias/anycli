<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Typed Exit Codes

`fsinspect` currently uses integer exit codes. In this step, you'll replace them with a shared enum so both commands return the same well-defined set of process exit codes.

## Add the exit code type

Add this enum to the project:

```csharp
public enum FsInspectExitCode
{
    Success = 0,
    FileNotFound = 1,
    CommandLineError = 2,
}
```

The enum values are the process exit codes returned by the application.

## Update InspectCommand

Change `InspectCommand` to use `NoParametersCommand<FsInspectExitCode>` and return the enum:

```csharp
public sealed class InspectCommand : NoParametersCommand<FsInspectExitCode>
{
    public override FsInspectExitCode Execute(CommandContext context)
    {
        var directory = new DirectoryInfo(Environment.CurrentDirectory);

        context.Console.Output.WriteLine($"Path:   {directory.FullName}");
        context.Console.Output.WriteLine($"Name:   {directory.Name}");
        context.Console.Output.WriteLine($"Parent: {directory.Parent?.FullName ?? "(none)"}");

        return FsInspectExitCode.Success;
    }
}
```

## Update PreviewCommand

Change `PreviewCommand` to implement `IAsyncCommand<PreviewParameters, FsInspectExitCode>`. Replace `return 1` with `return FsInspectExitCode.FileNotFound` and return `FsInspectExitCode.Success` at the end:

```csharp
public sealed class PreviewCommand : IAsyncCommand<PreviewParameters, FsInspectExitCode>
{
    public async Task<FsInspectExitCode> ExecuteAsync(
        CommandContext context,
        PreviewParameters parameters,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(parameters.Path))
        {
            context.Console.Error.WriteLine($"File not found: {parameters.Path}");
            return FsInspectExitCode.FileNotFound;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Line")
            .AddColumn("Text");

        var number = 0;
        await foreach (var line in File.ReadLinesAsync(parameters.Path, cancellationToken))
        {
            if (number == parameters.Lines) break;
            number++;
            table.AddRow(number.ToString(), Markup.Escape(line));
        }

        context.AnsiConsole.Output.Write(table);
        return FsInspectExitCode.Success;
    }
}
```

## Register the commands

Update the app registration to use `ConfigureCommands<FsInspectExitCode>` and set the command-line error exit code to `CommandLineError`:

```csharp
var app = ConsoleAppBuilder.Create()
    .WithName("fsinspect")
    .ConfigureErrorHandling(errors => errors
        .UseWriter<SpectreCommandErrorWriter>()
        .UseExitCode(FsInspectExitCode.CommandLineError))
    .ConfigureCommands<FsInspectExitCode>(commands => commands
        .AddCommand<InspectCommand>("info", "Show information about the current directory")
        .AddAsyncCommand<PreviewCommand, PreviewParameters>("preview", "Preview the first lines of a file"))
    .Build();
```

`ConfigureCommands<FsInspectExitCode>` requires every registered command to return that enum. `UseExitCode` maps command-line errors to `CommandLineError`, keeping them distinct from the file-not-found result returned by the command.

## Run the app

Successful commands return `FsInspectExitCode.Success`, which is exit code `0`:

```bash
dotnet run -- info
dotnet run -- preview sample.txt --lines 2
```

A missing file returns `FsInspectExitCode.FileNotFound`, which is exit code `1`:

```bash
dotnet run -- preview missing.txt
```

Command-line errors, such as an unknown option, return `CommandLineError`, which is exit code `2`:

```bash
dotnet run -- preview sample.txt --bad
```
