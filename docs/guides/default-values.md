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
    [DefaultValueDisplay("$HOME/.config/anycli/tasks.json")]
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

When a service provider is configured, the provider type is resolved from it. Register the concrete provider type:

```csharp
var services = new ServiceCollection();
services.AddSingleton<IAppPaths, AppPaths>();
services.AddSingleton<DataFileDefaultProvider>();

var app = ConsoleAppBuilder.Create()
    .UseServiceProvider(services.BuildServiceProvider())
    .Build();
```

When no service provider is configured, the provider needs a public parameterless constructor.

The provider is invoked only when the option or argument is omitted. It is not invoked when a command-line value is supplied.

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

The generator detects whether the provider implements the sync or async interface and awaits async defaults during parameter binding. Implementing both interfaces for the same value type is an error.

### Provider context

Async providers receive only a `CancellationToken`; sync providers receive no invocation arguments. Resolve dependencies through constructor injection; the provider type is resolved from the configured service provider when one is available.

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

Use display metadata to override the text or provide help for computed defaults:

```csharp
[Option("--data")]
[DefaultValueDisplay("tasks.json")]
[DefaultValueDisplayResource("DataFileDefault")]
public string DataFile { get; init; } = "tasks.json";
```

- `[DefaultValueDisplay]` supplies literal fallback text.
- `[DefaultValueDisplayResource]` supplies a localization resource key.
- When both are present, the resource key is resolved first and the literal text is used as the fallback.

Default display metadata overrides any captured initializer value. It is help-only and does not affect binding or invoke a default value provider.

## Priority

When a parameter is omitted, the order is:

1. Explicit command-line value.
2. `DefaultValueProvider`.
3. Property initializer.
4. `default(T)`.

## Diagnostics

The generator reports incorrect default configurations:

- `ANYCLI020` — the provider type is invalid.
- `ANYCLI022` — default display metadata is used on a required parameter.
- `ANYCLI023` — default display metadata is used without a provider or property initializer.
- `ANYCLI025` — a provider implements both sync and async interfaces.

## Related

- [Parameter attributes reference](../reference/parameter-attributes.md)
- [Diagnostics reference](../reference/diagnostics.md)
- [Define optional and variadic arguments](optional-and-variadic-arguments.md)
