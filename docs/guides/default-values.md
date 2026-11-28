<!--
SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>

SPDX-License-Identifier: MIT
-->

# Set default values

Use property initializers for defaults, and default value providers when the default must be computed at runtime.

## Options

```csharp
public sealed partial record ListParameters
{
    [Option("--status")]
    public string Status { get; init; } = "open";

    [Option("--count")]
    public int Count { get; init; } = 10;
}
```

## Optional arguments

```csharp
public sealed partial record CopyParameters
{
    [OptionalArgument(1)]
    public string Destination { get; init; } = "output.txt";
}
```

## Variadic arguments

```csharp
[VariadicArgument(1)]
public string[] Values { get; init; } = ["all"];
```

Defaults for variadic arguments are independent of `MinimumCount` and `MaximumCount` properties. When no values are supplied, the default remains. The minimum and maximum apply only to user-supplied values. When a default source exists, the argument is shown as optional in help output even if `MinimumCount` is greater than zero.

## Computed initializers

Initializers may run arbitrary code:

```csharp
[Option("--data")]
public string DataFile { get; init; } = GetDefaultDataFile();
```

Computed initializers are used for binding, but they are not evaluated while generating help output. Add default value display metadata when the default should appear in help.

## Default value providers

Use a provider when the default depends on services, configuration, the environment, or other runtime state:

```csharp
public sealed partial record SyncParameters
{
    [Option("--data", DefaultValueProvider = typeof(DataFileDefaultProvider))]
    [HelpDefaultValue("$HOME/.config/anycli/tasks.json")]
    public string DataFile { get; init; } = "";
}

public sealed class DataFileDefaultProvider : IDefaultValueProvider<string>
{
    private readonly IAppPaths _paths;

    public DataFileDefaultProvider(IAppPaths paths)
    {
        _paths = paths;
    }

    public string CreateDefault()
    {
        return Path.Combine(_paths.DataDirectory, "tasks.json");
    }
}
```

Providers are supported on:

- `[Option]`
- `[OptionalArgument]`
- `[VariadicArgument]`

Providers are not supported on `[Argument]` or `[RequiredOption]`.

When the provider type is registered in the configured service provider, that instance is used. Register the concrete provider type when it has constructor dependencies:

```csharp
var services = new ServiceCollection();
services.AddSingleton<IAppPaths, AppPaths>();
services.AddSingleton<DataFileDefaultProvider>();

var app = ConsoleAppBuilder.Create()
    .UseServiceProvider(services.BuildServiceProvider())
    .Build();
```

When the provider is not registered, it needs a public parameterless constructor.

The provider is invoked only when the option or argument is omitted. It is not invoked when a command-line value is supplied.

Providers may also be used on options declared in a `[GlobalOptions]` type. Default values are bound the same way for global options and command parameters.

### Async providers

Implement `IAsyncDefaultValueProvider<T>` when the default requires asynchronous work:

```csharp
public sealed class DataFileDefaultProvider : IAsyncDefaultValueProvider<string>
{
    private readonly IAppPaths _paths;

    public DataFileDefaultProvider(IAppPaths paths)
    {
        _paths = paths;
    }

    public async ValueTask<string> CreateDefaultAsync(CancellationToken cancellationToken)
    {
        var directory = await _paths.GetDataDirectoryAsync(cancellationToken);
        return Path.Combine(directory, "tasks.json");
    }
}
```

The generator detects whether the provider implements the sync or async interface and awaits async defaults during parameter binding. If a provider implements both interfaces, the async provider is used and `ANYCLI025` is reported as a warning.

## Help output

Simple initializers are captured by the generator and shown as the default in help output:

```csharp
[Option("--count")]
public int Count { get; init; } = 10;
```

```text
--count    Number of items [default: 10]
```

The generator captures literals, enum members, and simple array or collection initializers. It does not execute computed initializers for help.

Enum defaults use lower kebab-case by default:

```csharp
[Option("--mode")]
public OutputMode Mode { get; init; } = OutputMode.Json;
```

```text
--mode    Processing mode [default: json]
```

Formatting is configurable through `IHelpValueFormatter`:

```csharp
.ConfigureHelp(help => help
    .UseValueFormatter<MyHelpValueFormatter>())
```

This formatter applies only when no explicit default display text is configured.

Use `[HelpDefaultValue]` to override the text or provide help for computed defaults:

```csharp
[Option("--data")]
[HelpDefaultValue("tasks.json", ResourceKey = "DataFileDefault")]
public string DataFile { get; init; } = "tasks.json";
```

- `[HelpDefaultValue]` supplies literal fallback text, a localization resource key, or both.
- The resource key is resolved first; the literal text is used as the fallback.

`[HelpDefaultValue]` overrides any captured initializer value. It is help-only and does not affect binding or invoke a default value provider.

## Priority

When a parameter is omitted, the order is:

1. Explicit command-line value.
2. `DefaultValueProvider`.
3. Property initializer.
4. `default(T)`.

## Diagnostics

The generator reports incorrect default configurations:

- `ANYCLI020` — the provider type is invalid.
- `ANYCLI022` — help default value metadata on a required parameter is ignored.
- `ANYCLI023` — help default value metadata without a default source is ignored.
- `ANYCLI025` — a provider implements both sync and async interfaces; the async provider is used.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Diagnostics reference](../reference/diagnostics.md)
- [Define optional and variadic arguments](optional-and-variadic-arguments.md)
