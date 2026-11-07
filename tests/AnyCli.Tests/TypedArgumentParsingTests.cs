using System.Globalization;

namespace AnyCli.Tests;

public class TypedArgumentParsingTests
{
    [Test]
    public async Task ParsesScalarArguments()
    {
        var app = CreateApp<ScalarCommand, ScalarSettings>("parse");
        var id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        var result = await Run(app, ["parse", "42", "true", id.ToString(), "2026-11-07", "very-slow", "3"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo($"42|True|{id}|2026-11-07|VerySlow|3");
    }

    [Test]
    public async Task OptionalValue_IsNullWhenNotSupplied()
    {
        var app = CreateApp<ScalarCommand, ScalarSettings>("parse");
        var id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e");

        var result = await Run(app, ["parse", "1", "false", id.ToString(), "2026-11-07", "fast"]);

        await Assert.That(result.Output).IsEqualTo($"1|False|{id}|2026-11-07|Fast|null");
    }

    [Test]
    public async Task MissingRequiredArgument_ThrowsCommandArgumentException()
    {
        var app = CreateApp<ScalarCommand, ScalarSettings>("parse");
        CommandArgumentException? exception = null;

        try
        {
            await Run(app, ["parse"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'count'");
    }

    [Test]
    public async Task InvalidValue_ThrowsCommandArgumentException()
    {
        var app = CreateApp<ScalarCommand, ScalarSettings>("parse");
        CommandArgumentException? exception = null;

        try
        {
            await Run(app, ["parse", "nope", "true", Guid.Empty.ToString(), "2026-11-07", "fast"]);
        }
        catch (CommandArgumentException ex)
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
        var app = CreateApp<SumCommand, SumSettings>("sum");

        var result = await Run(app, ["sum", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("6");
    }

    [Test]
    public async Task VariadicArguments_AllowZeroValues()
    {
        var app = CreateApp<SumCommand, SumSettings>("sum");

        var result = await Run(app, ["sum"]);

        await Assert.That(result.Output).IsEqualTo("0");
    }

    [Test]
    public async Task OneOrMoreVariadic_RequiresAtLeastOneValue()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<OneOrMoreCommand, OneOrMoreSettings>("tags"))
            .Build();

        CommandArgumentException? exception = null;
        try
        {
            await Run(app, ["tags"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        var result = await Run(app, ["tags", "a", "b"]);

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'tags'");
        await Assert.That(result.Output).IsEqualTo("a,b");
    }

    [Test]
    public async Task ParsesIParsableOnlyTypes()
    {
        var app = CreateApp<ParsableOnlyCommand, ParsableOnlySettings>("parsable");

        var result = await Run(app, ["parsable", "value:hello"]);

        await Assert.That(result.Output).IsEqualTo("hello");
    }

    [Test]
    public async Task ParsesCustomTypesWithParser()
    {
        var app = CreateApp<WidgetCommand, WidgetSettings>("widget");

        var result = await Run(app, ["widget", "widget:hello"]);

        await Assert.That(result.Output).IsEqualTo("hello");
    }

    [Test]
    public async Task CustomParserFailure_ThrowsCommandArgumentException()
    {
        var app = CreateApp<WidgetCommand, WidgetSettings>("widget");
        CommandArgumentException? exception = null;

        try
        {
            await Run(app, ["widget", "nope"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'widget'");
        await Assert.That(exception.Message).Contains("nope");
    }

    private static ConsoleApp CreateApp<TCommand, TSettings>(string name)
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings> =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TSettings>(name))
            .Build();

    private static async Task<RunResult> Run(ConsoleApp app, string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class ScalarCommand : ICommand<ScalarSettings>
    {
        public int Execute(CommandContext context, ScalarSettings settings)
        {
            context.Console.Output.Write(
                $"{settings.Count}|{settings.Force}|{settings.Id}|" +
                $"{settings.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}|" +
                $"{settings.Mode}|{settings.Retries?.ToString(CultureInfo.InvariantCulture) ?? "null"}");
            return 0;
        }
    }

    private sealed class SumCommand : ICommand<SumSettings>
    {
        public int Execute(CommandContext context, SumSettings settings)
        {
            context.Console.Output.Write(settings.Numbers.Sum().ToString(CultureInfo.InvariantCulture));
            return 0;
        }
    }

    private sealed class WidgetCommand : ICommand<WidgetSettings>
    {
        public int Execute(CommandContext context, WidgetSettings settings)
        {
            context.Console.Output.Write(settings.Widget.Name);
            return 0;
        }
    }

    private sealed class OneOrMoreCommand : ICommand<OneOrMoreSettings>
    {
        public int Execute(CommandContext context, OneOrMoreSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Tags));
            return 0;
        }
    }

    private sealed class ParsableOnlyCommand : ICommand<ParsableOnlySettings>
    {
        public int Execute(CommandContext context, ParsableOnlySettings settings)
        {
            context.Console.Output.Write(settings.Value.Value);
            return 0;
        }
    }
}

public sealed class OneOrMoreSettings : ICommandSettings<OneOrMoreSettings>
{
    public string[] Tags { get; set; } = [];

    public static void Configure(CommandArgumentBuilder arguments) =>
        arguments.AddVariadic<string>("tags", ArgumentArity.OneOrMore);

    public static OneOrMoreSettings Bind(CommandArguments arguments) =>
        new() { Tags = [.. arguments.GetVariadic("tags")] };
}

public sealed partial class ScalarSettings
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

    [Argument(5, Optional = true)]
    public int? Retries { get; set; }
}

public sealed partial class SumSettings
{
    [VariadicArgument(0)]
    public int[] Numbers { get; set; } = [];
}

public sealed partial class WidgetSettings
{
    [Argument(0, Parser = typeof(WidgetParser))]
    public Widget Widget { get; set; } = new(string.Empty);
}

public sealed partial class ParsableOnlySettings
{
    [Argument(0)]
    public ParsableOnly Value { get; set; } = new(string.Empty);
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

public enum ParseMode
{
    Fast,
    Slow,
    VerySlow,
}

public sealed record Widget(string Name);

public sealed class WidgetParser : IArgumentParser<Widget>
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
