// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli;
using AnyCli.Tests.Fixtures;

[assembly: ArgumentParser<AnyCli.Tests.PrecedenceValue, AnyCli.Tests.AssemblyPrecedenceParser>]
[assembly: ArgumentParser<AnyCli.Tests.SpanPrecedenceValue, AnyCli.Tests.AssemblySpanPrecedenceParser>]
[assembly: ArgumentParser<AnyCli.Tests.PrecedenceMode, AnyCli.Tests.AssemblyEnumPrecedenceParser>]

namespace AnyCli.Tests;

public class ParsingPrecedenceTests
{
    [Test]
    public async Task AssemblyRegistration_WinsOverBuiltInParsable()
    {
        var app = CreateApp<AssemblyValueCommand, AssemblyValueSettings>("value");

        var result = await TestAppHarness.RunAsync(app, ["value", "input"]);

        await Assert.That(result.Output).IsEqualTo("assembly:input");
    }

    [Test]
    public async Task AssemblyRegistration_WinsOverISpanParsable()
    {
        var app = CreateApp<AssemblySpanValueCommand, AssemblySpanValueSettings>("value");

        var result = await TestAppHarness.RunAsync(app, ["value", "input"]);

        await Assert.That(result.Output).IsEqualTo("assembly-span:input");
    }

    [Test]
    public async Task PropertyParser_WinsOverAssemblyRegistration()
    {
        var app = CreateApp<PropertyValueCommand, PropertyValueSettings>("value");

        var result = await TestAppHarness.RunAsync(app, ["value", "input"]);

        await Assert.That(result.Output).IsEqualTo("property:input");
    }

    [Test]
    public async Task AssemblyRegistration_WinsOverGlobalEnumParser()
    {
        var app = CreateApp<AssemblyEnumCommand, AssemblyEnumSettings>("mode");

        var result = await TestAppHarness.RunAsync(app, ["mode", "whatever"]);

        await Assert.That(result.Output).IsEqualTo("Assembly");
    }

    [Test]
    public async Task PropertyParser_WinsOverAssemblyEnumRegistration()
    {
        var app = CreateApp<PropertyEnumCommand, PropertyEnumSettings>("mode");

        var result = await TestAppHarness.RunAsync(app, ["mode", "whatever"]);

        await Assert.That(result.Output).IsEqualTo("Property");
    }

    private static ConsoleApp CreateApp<TCommand, TSettings>(string name)
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings> =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TSettings>(name))
            .Build();

    private sealed class AssemblyValueCommand : ICommand<AssemblyValueSettings>
    {
        public int Execute(CommandContext context, AssemblyValueSettings settings)
        {
            context.Console.Output.Write(settings.Value.Source);
            return 0;
        }
    }

    private sealed class AssemblySpanValueCommand : ICommand<AssemblySpanValueSettings>
    {
        public int Execute(CommandContext context, AssemblySpanValueSettings settings)
        {
            context.Console.Output.Write(settings.Value.Source);
            return 0;
        }
    }

    private sealed class PropertyValueCommand : ICommand<PropertyValueSettings>
    {
        public int Execute(CommandContext context, PropertyValueSettings settings)
        {
            context.Console.Output.Write(settings.Value.Source);
            return 0;
        }
    }

    private sealed class AssemblyEnumCommand : ICommand<AssemblyEnumSettings>
    {
        public int Execute(CommandContext context, AssemblyEnumSettings settings)
        {
            context.Console.Output.Write(settings.Mode.ToString());
            return 0;
        }
    }

    private sealed class PropertyEnumCommand : ICommand<PropertyEnumSettings>
    {
        public int Execute(CommandContext context, PropertyEnumSettings settings)
        {
            context.Console.Output.Write(settings.Mode.ToString());
            return 0;
        }
    }
}

public sealed partial class AssemblyValueSettings
{
    [Argument(0)]
    public PrecedenceValue Value { get; set; } = new(string.Empty);
}

public sealed partial class AssemblySpanValueSettings
{
    [Argument(0)]
    public SpanPrecedenceValue Value { get; set; } = new(string.Empty);
}

public sealed partial class PropertyValueSettings
{
    [Argument(0, Parser = typeof(PropertyPrecedenceParser))]
    public PrecedenceValue Value { get; set; } = new(string.Empty);
}

public sealed partial class AssemblyEnumSettings
{
    [Argument(0)]
    public PrecedenceMode Mode { get; set; }
}

public sealed partial class PropertyEnumSettings
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

public sealed class AssemblySpanPrecedenceParser : IArgumentParser<SpanPrecedenceValue>
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

public sealed class AssemblyPrecedenceParser : IArgumentParser<PrecedenceValue>
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

public sealed class PropertyPrecedenceParser : IArgumentParser<PrecedenceValue>
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

public sealed class AssemblyEnumPrecedenceParser : IArgumentParser<PrecedenceMode>
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

public sealed class PropertyEnumPrecedenceParser : IArgumentParser<PrecedenceMode>
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
