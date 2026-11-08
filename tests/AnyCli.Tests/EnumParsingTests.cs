// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli;
using AnyCli.Tests.Fixtures;

[assembly: EnumArgumentParser<AnyCli.Tests.TestEnumArgumentParser>]

namespace AnyCli.Tests;

public class EnumParsingTests
{
    [Test]
    public async Task DefaultEnumArgumentParser_MatchesKebabCase()
    {
        var success = DefaultEnumArgumentParser.TryParse<KebabMode>(
            "very-slow",
            provider: null,
            out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesExactCase()
    {
        var result = ArgumentParsing.ParseEnum<KebabMode>("VerySlow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesLowercaseNames()
    {
        var result = ArgumentParsing.ParseEnum<KebabMode>("veryslow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesKebabCase()
    {
        var result = ArgumentParsing.ParseEnum<KebabMode>("very-slow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesMixedCaseKebabValues()
    {
        var result = ArgumentParsing.ParseEnum<KebabMode>("Very-Slow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task GeneratedSettings_UseDefaultEnumParsing()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumSettings>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "very-slow"]);

        await Assert.That(result.Output).IsEqualTo("VerySlow");
    }

    [Test]
    public async Task AssemblyEnumArgumentParser_OverridesDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumSettings>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "custom"]);

        await Assert.That(result.Output).IsEqualTo("VerySlow");
    }

    private sealed class GlobalEnumCommand : ICommand<GlobalEnumSettings>
    {
        public int Execute(CommandContext context, GlobalEnumSettings settings)
        {
            context.Console.Output.Write(settings.Mode.ToString());
            return 0;
        }
    }
}

public sealed partial class GlobalEnumSettings
{
    [Argument(0)]
    public KebabMode Mode { get; set; }
}

public sealed class TestEnumArgumentParser : IEnumArgumentParser
{
    public static bool TryParse<TEnum>(
        string value,
        IFormatProvider? provider,
        out TEnum result)
        where TEnum : struct, Enum
    {
        if (typeof(TEnum) == typeof(KebabMode) && value == "custom")
        {
            result = (TEnum)(object)KebabMode.VerySlow;
            return true;
        }

        return DefaultEnumArgumentParser.TryParse(value, provider, out result);
    }
}

public enum KebabMode
{
    Fast,
    VerySlow,
}
