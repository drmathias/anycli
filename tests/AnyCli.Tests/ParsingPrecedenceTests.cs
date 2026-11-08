// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli;
using AnyCli.Tests.Fixtures;

[assembly: ParameterValueParser<AnyCli.Tests.PrecedenceValue, AnyCli.Tests.AssemblyPrecedenceParser>]
[assembly: ParameterValueParser<AnyCli.Tests.SpanPrecedenceValue, AnyCli.Tests.AssemblySpanPrecedenceParser>]
[assembly: ParameterValueParser<AnyCli.Tests.PrecedenceMode, AnyCli.Tests.AssemblyEnumPrecedenceParser>]

namespace AnyCli.Tests;

public class ParsingPrecedenceTests
{
    [Test]
    public async Task AssemblyRegistration_WinsOverBuiltInParsable()
    {
        var app = CreateApp<AssemblyValueCommand, AssemblyValueParameters>("value");

        var result = await TestAppHarness.RunAsync(app, ["value", "input"]);

        await Assert.That(result.Output).IsEqualTo("assembly:input");
    }

    [Test]
    public async Task AssemblyRegistration_WinsOverISpanParsable()
    {
        var app = CreateApp<AssemblySpanValueCommand, AssemblySpanValueParameters>("value");

        var result = await TestAppHarness.RunAsync(app, ["value", "input"]);

        await Assert.That(result.Output).IsEqualTo("assembly-span:input");
    }

    [Test]
    public async Task PropertyParser_WinsOverAssemblyRegistration()
    {
        var app = CreateApp<PropertyValueCommand, PropertyValueParameters>("value");

        var result = await TestAppHarness.RunAsync(app, ["value", "input"]);

        await Assert.That(result.Output).IsEqualTo("property:input");
    }

    [Test]
    public async Task AssemblyRegistration_WinsOverGlobalEnumParser()
    {
        var app = CreateApp<AssemblyEnumCommand, AssemblyEnumParameters>("mode");

        var result = await TestAppHarness.RunAsync(app, ["mode", "whatever"]);

        await Assert.That(result.Output).IsEqualTo("Assembly");
    }

    [Test]
    public async Task PropertyParser_WinsOverAssemblyEnumRegistration()
    {
        var app = CreateApp<PropertyEnumCommand, PropertyEnumParameters>("mode");

        var result = await TestAppHarness.RunAsync(app, ["mode", "whatever"]);

        await Assert.That(result.Output).IsEqualTo("Property");
    }

    private static ConsoleApp CreateApp<TCommand, TParameters>(string name)
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, ICommandParameters<TParameters> =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TParameters>(name))
            .Build();

    private sealed class AssemblyValueCommand : ICommand<AssemblyValueParameters>
    {
        public int Execute(CommandContext context, AssemblyValueParameters parameters)
        {
            context.Console.Output.Write(parameters.Value.Source);
            return 0;
        }
    }

    private sealed class AssemblySpanValueCommand : ICommand<AssemblySpanValueParameters>
    {
        public int Execute(CommandContext context, AssemblySpanValueParameters parameters)
        {
            context.Console.Output.Write(parameters.Value.Source);
            return 0;
        }
    }

    private sealed class PropertyValueCommand : ICommand<PropertyValueParameters>
    {
        public int Execute(CommandContext context, PropertyValueParameters parameters)
        {
            context.Console.Output.Write(parameters.Value.Source);
            return 0;
        }
    }

    private sealed class AssemblyEnumCommand : ICommand<AssemblyEnumParameters>
    {
        public int Execute(CommandContext context, AssemblyEnumParameters parameters)
        {
            context.Console.Output.Write(parameters.Mode.ToString());
            return 0;
        }
    }

    private sealed class PropertyEnumCommand : ICommand<PropertyEnumParameters>
    {
        public int Execute(CommandContext context, PropertyEnumParameters parameters)
        {
            context.Console.Output.Write(parameters.Mode.ToString());
            return 0;
        }
    }
}

public sealed partial class AssemblyValueParameters
{
    [Argument(0)]
    public PrecedenceValue Value { get; set; } = new(string.Empty);
}

public sealed partial class AssemblySpanValueParameters
{
    [Argument(0)]
    public SpanPrecedenceValue Value { get; set; } = new(string.Empty);
}

public sealed partial class PropertyValueParameters
{
    [Argument(0, Parser = typeof(PropertyPrecedenceParser))]
    public PrecedenceValue Value { get; set; } = new(string.Empty);
}

public sealed partial class AssemblyEnumParameters
{
    [Argument(0)]
    public PrecedenceMode Mode { get; set; }
}

public sealed partial class PropertyEnumParameters
{
    [Argument(0, Parser = typeof(PropertyEnumPrecedenceParser))]
    public PrecedenceMode Mode { get; set; }
}

public sealed record PrecedenceValue(string Source) : IParsable<PrecedenceValue>
{
    public static PrecedenceValue Parse(string s, IFormatProvider? provider) =>
        new($"builtin:{s}");

    public static bool TryParse(string? s, IFormatProvider? provider, out PrecedenceValue result)
    {
        result = new PrecedenceValue($"builtin:{s}");
        return s is not null;
    }
}

public sealed record SpanPrecedenceValue(string Source) : ISpanParsable<SpanPrecedenceValue>
{
    public static SpanPrecedenceValue Parse(string s, IFormatProvider? provider) =>
        new($"parsable:{s}");

    public static bool TryParse(
        string? s,
        IFormatProvider? provider,
        out SpanPrecedenceValue result)
    {
        result = new SpanPrecedenceValue($"parsable:{s}");
        return s is not null;
    }

    public static SpanPrecedenceValue Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        new($"span:{s}");

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out SpanPrecedenceValue result)
    {
        result = new SpanPrecedenceValue($"span:{s}");
        return true;
    }
}

public sealed class AssemblySpanPrecedenceParser : IParameterParser<SpanPrecedenceValue>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out SpanPrecedenceValue result)
    {
        result = new SpanPrecedenceValue($"assembly-span:{value}");
        return true;
    }
}

public sealed class AssemblyPrecedenceParser : IParameterParser<PrecedenceValue>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out PrecedenceValue result)
    {
        result = new PrecedenceValue($"assembly:{value}");
        return true;
    }
}

public sealed class PropertyPrecedenceParser : IParameterParser<PrecedenceValue>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out PrecedenceValue result)
    {
        result = new PrecedenceValue($"property:{value}");
        return true;
    }
}

public enum PrecedenceMode
{
    None,
    Assembly,
    Property,
}

public sealed class AssemblyEnumPrecedenceParser : IParameterParser<PrecedenceMode>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out PrecedenceMode result)
    {
        result = PrecedenceMode.Assembly;
        return true;
    }
}

public sealed class PropertyEnumPrecedenceParser : IParameterParser<PrecedenceMode>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out PrecedenceMode result)
    {
        result = PrecedenceMode.Property;
        return true;
    }
}
