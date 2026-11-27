// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

[assembly: ParameterValueParser<AnyCli.Tests.AsyncRegisteredWidget, AnyCli.Tests.AsyncRegisteredWidgetParser>]

namespace AnyCli.Tests;

public class AsyncParameterParserTests
{
    [Test]
    public async Task ExplicitAsyncParser_UsesServices()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new AsyncWidgetParser(new AsyncWidgetPrefix("async:")));
        serviceProvider.Add(() => new AsyncWidgetCommand());

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncWidgetCommand, AsyncWidgetParameters>("widget"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["widget", "value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("async:value");
    }

    [Test]
    public async Task ExplicitAsyncParser_FailureWritesError()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncWidgetCommand, AsyncWidgetParameters>("widget"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["widget", "bad"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("'widget'");
        await Assert.That(result.Error).Contains("bad");
    }

    [Test]
    public async Task ExplicitAsyncParser_ParsesVariadicArguments()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncWidgetVariadicCommand, AsyncWidgetVariadicParameters>("widgets"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["widgets", "one", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("one,two");
    }

    [Test]
    public async Task AsyncParser_WorksForGlobalOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddGlobalOptions<AsyncGlobalOptions>()
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncGlobalCommand, AsyncGlobalParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--widget", "value", "run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("value|value");
    }

    [Test]
    public async Task AssemblyRegisteredAsyncParser_IsUsed()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncRegisteredWidgetCommand, AsyncRegisteredWidgetParameters>("registered"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["registered", "value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("registered:value");
    }
}

public sealed record AsyncWidget(string Name);

public sealed class AsyncWidgetPrefix(string value)
{
    public string Value { get; } = value;
}

public sealed class AsyncWidgetParser : IAsyncParameterParser<AsyncWidget>
{
    private readonly AsyncWidgetPrefix? _prefix;

    public AsyncWidgetParser()
    {
    }

    public AsyncWidgetParser(AsyncWidgetPrefix prefix)
    {
        _prefix = prefix;
    }

    public async ValueTask<ParameterParseResult<AsyncWidget>> TryParseAsync(
        string value,
        IFormatProvider? provider,
        CancellationToken cancellationToken)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        return value.StartsWith("bad", StringComparison.Ordinal)
            ? ParameterParseResult<AsyncWidget>.Failed
            : ParameterParseResult<AsyncWidget>.Succeeded(new AsyncWidget((_prefix?.Value ?? string.Empty) + value));
    }
}

public sealed partial class AsyncWidgetParameters
{
    [Argument(0, Parser = typeof(AsyncWidgetParser))]
    public AsyncWidget Widget { get; init; } = new(string.Empty);
}

public sealed partial class AsyncWidgetVariadicParameters
{
    [VariadicArgument(0, Parser = typeof(AsyncWidgetParser))]
    public AsyncWidget[] Widgets { get; init; } = [];
}

public readonly record struct AsyncRegisteredWidget(string Name);

public sealed class AsyncRegisteredWidgetParser : IAsyncParameterParser<AsyncRegisteredWidget>
{
    public ValueTask<ParameterParseResult<AsyncRegisteredWidget>> TryParseAsync(
        string value,
        IFormatProvider? provider,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(
            ParameterParseResult<AsyncRegisteredWidget>.Succeeded(
                new AsyncRegisteredWidget("registered:" + value)));
    }
}

public sealed partial class AsyncRegisteredWidgetParameters
{
    [Argument(0)]
    public AsyncRegisteredWidget Widget { get; init; }
}

internal sealed class AsyncWidgetCommand : ICommand<AsyncWidgetParameters>
{
    public int Execute(CommandContext context, AsyncWidgetParameters parameters)
    {
        context.Console.Output.Write(parameters.Widget.Name);
        return 0;
    }
}

internal sealed class AsyncWidgetVariadicCommand : ICommand<AsyncWidgetVariadicParameters>
{
    public int Execute(CommandContext context, AsyncWidgetVariadicParameters parameters)
    {
        context.Console.Output.Write(string.Join(",", parameters.Widgets.Select(widget => widget.Name)));
        return 0;
    }
}

internal sealed class AsyncRegisteredWidgetCommand : ICommand<AsyncRegisteredWidgetParameters>
{
    public int Execute(CommandContext context, AsyncRegisteredWidgetParameters parameters)
    {
        context.Console.Output.Write(parameters.Widget.Name);
        return 0;
    }
}


[GlobalOptions]
public partial class AsyncGlobalOptions
{
    [Option("--widget", Parser = typeof(AsyncWidgetParser))]
    public AsyncWidget Widget { get; init; } = new(string.Empty);
}

public sealed partial class AsyncGlobalParameters : AsyncGlobalOptions
{
}

internal sealed class AsyncGlobalCommand : ICommand<AsyncGlobalParameters>
{
    public int Execute(CommandContext context, AsyncGlobalParameters parameters)
    {
        var globalOptions = ((CommandContext<AsyncGlobalOptions>)context).GlobalOptions;
        context.Console.Output.Write($"{globalOptions.Widget.Name}|{parameters.Widget.Name}");
        return 0;
    }
}
