// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

using AnyCli.Tests.Fixtures;

[assembly: ParameterValueParser<AnyCli.Tests.RegisteredWidget, AnyCli.Tests.RegisteredWidgetParser>]

namespace AnyCli.Tests;

public class TypedParameterValueParsingTests
{
    [Test]
    public async Task ParsesScalarArguments()
    {
        var app = CreateApp<ScalarCommand, ScalarParameters>("parse");
        var id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        var result = await TestAppHarness.RunAsync(app, ["parse", "42", "true", id.ToString(), "2026-11-07", "very-slow", "3"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo($"42|True|{id}|2026-11-07|VerySlow|3");
    }

    [Test]
    public async Task OptionalValue_IsNullWhenNotSupplied()
    {
        var app = CreateApp<ScalarCommand, ScalarParameters>("parse");
        var id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        var result = await TestAppHarness.RunAsync(app, ["parse", "1", "false", id.ToString(), "2026-11-07", "fast"]);

        await Assert.That(result.Output).IsEqualTo($"1|False|{id}|2026-11-07|Fast|null");
    }

    [Test]
    public async Task MissingRequiredArgument_ThrowsCommandParameterException()
    {
        var app = CreateApp<ScalarCommand, ScalarParameters>("parse");
        CommandParameterException? exception = null;

        try
        {
            await TestAppHarness.RunAsync(app, ["parse"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'count'");
    }

    [Test]
    public async Task InvalidValue_ThrowsCommandParameterException()
    {
        var app = CreateApp<ScalarCommand, ScalarParameters>("parse");
        CommandParameterException? exception = null;

        try
        {
            await TestAppHarness.RunAsync(app, ["parse", "nope", "true", Guid.Empty.ToString(), "2026-11-07", "fast"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'count'");
        await Assert.That(exception.Message).Contains("nope");
    }

    [Test]
    public async Task ParsesVariadicArguments()
    {
        var app = CreateApp<SumCommand, SumParameters>("sum");

        var result = await TestAppHarness.RunAsync(app, ["sum", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("6");
    }

    [Test]
    public async Task VariadicArguments_AllowZeroValues()
    {
        var app = CreateApp<SumCommand, SumParameters>("sum");

        var result = await TestAppHarness.RunAsync(app, ["sum"]);

        await Assert.That(result.Output).IsEqualTo("0");
    }

    [Test]
    public async Task OneOrMoreVariadic_RequiresAtLeastOneValue()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<OneOrMoreCommand, OneOrMoreParameters>("tags"))
            .Build();

        CommandParameterException? exception = null;
        try
        {
            await TestAppHarness.RunAsync(app, ["tags"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        var result = await TestAppHarness.RunAsync(app, ["tags", "a", "b"]);

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'tags'");
        await Assert.That(result.Output).IsEqualTo("a,b");
    }

    [Test]
    public async Task ParsesIParsableOnlyTypes()
    {
        var app = CreateApp<ParsableOnlyCommand, ParsableOnlyParameters>("parsable");

        var result = await TestAppHarness.RunAsync(app, ["parsable", "value:hello"]);

        await Assert.That(result.Output).IsEqualTo("hello");
    }

    [Test]
    public async Task ParsesTypesRegisteredWithAssemblyAttribute()
    {
        var app = CreateApp<RegisteredWidgetCommand, RegisteredWidgetParameters>("registered");

        var result = await TestAppHarness.RunAsync(app, ["registered", "widget:hello"]);

        await Assert.That(result.Output).IsEqualTo("hello");
    }

    [Test]
    public async Task ParsesCustomTypesWithParser()
    {
        var app = CreateApp<WidgetCommand, WidgetParameters>("widget");

        var result = await TestAppHarness.RunAsync(app, ["widget", "widget:hello"]);

        await Assert.That(result.Output).IsEqualTo("hello");
    }

    [Test]
    public async Task CustomParserFailure_ThrowsCommandParameterException()
    {
        var app = CreateApp<WidgetCommand, WidgetParameters>("widget");
        CommandParameterException? exception = null;

        try
        {
            await TestAppHarness.RunAsync(app, ["widget", "nope"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'widget'");
        await Assert.That(exception.Message).Contains("nope");
    }

    private static ConsoleApp CreateApp<TCommand, TParameters>(string name)
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, ICommandParameters<TParameters> =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TParameters>(name))
            .Build();

    [Test]
    public async Task CustomParameterParser_AppliesToOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands => commands.AddCommand<OptionWidgetCommand, OptionWidgetParameters>("parse"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["parse", "--widget", "widget:value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("value");
    }

    private sealed class OptionWidgetCommand : ICommand<OptionWidgetParameters>
    {
        public int Execute(CommandContext context, OptionWidgetParameters parameters)
        {
            context.Console.Output.Write(parameters.Widget.Name);
            return 0;
        }
    }

    private sealed class ScalarCommand : ICommand<ScalarParameters>
    {
        public int Execute(CommandContext context, ScalarParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Count}|{parameters.Force}|{parameters.Id}|" +
                $"{parameters.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}|" +
                $"{parameters.Mode}|{parameters.Retries?.ToString(CultureInfo.InvariantCulture) ?? "null"}");
            return 0;
        }
    }

    private sealed class SumCommand : ICommand<SumParameters>
    {
        public int Execute(CommandContext context, SumParameters parameters)
        {
            context.Console.Output.Write(parameters.Numbers.Sum().ToString(CultureInfo.InvariantCulture));
            return 0;
        }
    }

    private sealed class WidgetCommand : ICommand<WidgetParameters>
    {
        public int Execute(CommandContext context, WidgetParameters parameters)
        {
            context.Console.Output.Write(parameters.Widget.Name);
            return 0;
        }
    }

    private sealed class OneOrMoreCommand : ICommand<OneOrMoreParameters>
    {
        public int Execute(CommandContext context, OneOrMoreParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Tags));
            return 0;
        }
    }

    private sealed class ParsableOnlyCommand : ICommand<ParsableOnlyParameters>
    {
        public int Execute(CommandContext context, ParsableOnlyParameters parameters)
        {
            context.Console.Output.Write(parameters.Value.Value);
            return 0;
        }
    }

    private sealed class RegisteredWidgetCommand : ICommand<RegisteredWidgetParameters>
    {
        public int Execute(CommandContext context, RegisteredWidgetParameters parameters)
        {
            context.Console.Output.Write(parameters.Widget.Name);
            return 0;
        }
    }
}

public sealed class OneOrMoreParameters : ICommandParameters<OneOrMoreParameters>
{
    public string[] Tags { get; set; } = [];

    public static void Configure(CommandParameterDefinitionBuilder parameters) =>
        parameters.Arguments.AddVariadic<string>("tags", ArgumentArity.OneOrMore);

    public static OneOrMoreParameters Bind(
        ParsedCommandParameters parameters,
        CommandOptionValues globalOptionValues) =>
        new() { Tags = [.. parameters.Arguments.GetVariadic("tags")] };
}

public sealed partial class ScalarParameters
{
    [Argument(0)]
    public int Count { get; set; }

    [Argument(1)]
    public bool Force { get; set; }

    [Argument(2)]
    public Guid Id { get; set; }

    [Argument(3)]
    public DateTime Date { get; set; }

    [Argument(4)]
    public ParseMode Mode { get; set; }

    [OptionalArgument(5)]
    public int? Retries { get; set; }
}

public sealed partial class SumParameters
{
    [VariadicArgument(0)]
    public int[] Numbers { get; set; } = [];
}

public sealed partial class WidgetParameters
{
    [Argument(0, Parser = typeof(WidgetParser))]
    public Widget Widget { get; set; } = new(string.Empty);
}

public sealed partial class ParsableOnlyParameters
{
    [Argument(0)]
    public ParsableOnly Value { get; set; } = new(string.Empty);
}

public sealed partial class OptionWidgetParameters
{
    [Option("widget", Parser = typeof(WidgetParser))]
    public Widget Widget { get; set; } = new(string.Empty);
}

public sealed partial class RegisteredWidgetParameters
{
    [Argument(0)]
    public RegisteredWidget Widget { get; set; } = new(string.Empty);
}

public sealed record ParsableOnly(string Value) : IParsable<ParsableOnly>
{
    public static ParsableOnly Parse(string s, IFormatProvider? provider) =>
        TryParse(s, provider, out var result) ? result : throw new FormatException();

    public static bool TryParse(string? s, IFormatProvider? provider, out ParsableOnly result)
    {
        const string prefix = "value:";
        if (s is not null && s.StartsWith(prefix, StringComparison.Ordinal))
        {
            result = new ParsableOnly(s[prefix.Length..]);
            return true;
        }

        result = null!;
        return false;
    }
}

public sealed record RegisteredWidget(string Name);

public sealed class RegisteredWidgetParser : IParameterParser<RegisteredWidget>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out RegisteredWidget result)
    {
        const string prefix = "widget:";
        if (value.StartsWith(prefix, StringComparison.Ordinal))
        {
            result = new RegisteredWidget(value[prefix.Length..]);
            return true;
        }

        result = null!;
        return false;
    }
}

public enum ParseMode
{
    Fast,
    Slow,
    VerySlow,
}

public sealed record Widget(string Name);

public sealed class WidgetParser : IParameterParser<Widget>
{
    public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
    {
        const string prefix = "widget:";
        if (value.StartsWith(prefix, StringComparison.Ordinal))
        {
            result = new Widget(value[prefix.Length..]);
            return true;
        }

        result = null!;
        return false;
    }
}
