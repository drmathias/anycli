// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

[assembly: EnumParameterParser<AnyCli.Tests.TestEnumParameterParser>]

namespace AnyCli.Tests;

public class EnumParsingTests
{
    [Test]
    public async Task DefaultEnumParameterParser_MatchesKebabCase()
    {
        var success = DefaultEnumParameterParser.TryParse<KebabMode>(
            "very-slow",
            provider: null,
            out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task DefaultEnumParameterParser_RejectsNumericValues()
    {
        foreach (var value in new[] { "0", "1", "-1", "+1", "0x1", "999", "1,2" })
        {
            var success = DefaultEnumParameterParser.TryParse<KebabMode>(
                value,
                provider: null,
                out _);

            await Assert.That(success).IsFalse();
        }
    }

    [Test]
    public async Task ParseEnum_RejectsNumericValues()
    {
        CommandErrorException? exception = null;

        try
        {
            _ = ParameterValueParsing.ParseEnum<KebabMode>("1", "mode");
        }
        catch (CommandErrorException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Error is InvalidParameterValueError).IsTrue();

        var error = (InvalidParameterValueError)exception.Error;
        await Assert.That(error.Value).IsEqualTo("1");
        await Assert.That(error.ParameterName).IsEqualTo("mode");
    }

    [Test]
    public async Task GeneratedParameters_RejectNumericEnumValues()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumParameters>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "1"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("Invalid value '1'");
    }

    [Test]
    public async Task ParseEnum_MatchesExactCase()
    {
        var result = ParameterValueParsing.ParseEnum<KebabMode>("VerySlow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesLowercaseNames()
    {
        var result = ParameterValueParsing.ParseEnum<KebabMode>("veryslow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesKebabCase()
    {
        var result = ParameterValueParsing.ParseEnum<KebabMode>("very-slow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task ParseEnum_MatchesMixedCaseKebabValues()
    {
        var result = ParameterValueParsing.ParseEnum<KebabMode>("Very-Slow", "mode");

        await Assert.That(result).IsEqualTo(KebabMode.VerySlow);
    }

    [Test]
    public async Task GeneratedParameters_UseDefaultEnumParsing()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumParameters>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "very-slow"]);

        await Assert.That(result.Output).IsEqualTo("VerySlow");
    }

    [Test]
    public async Task AssemblyEnumParameterParser_OverridesDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumParameters>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "custom"]);

        await Assert.That(result.Output).IsEqualTo("VerySlow");
    }

    private sealed class GlobalEnumCommand : ICommand<GlobalEnumParameters>
    {
        public int Execute(CommandContext context, GlobalEnumParameters parameters)
        {
            context.Console.Output.Write(parameters.Mode.ToString());
            return 0;
        }
    }
}

public sealed partial class GlobalEnumParameters
{
    [Argument(0)]
    public KebabMode Mode { get; set; }
}

public sealed class TestEnumParameterParser : IEnumParameterParser
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

        return DefaultEnumParameterParser.TryParse(value, provider, out result);
    }
}

public enum KebabMode
{
    Fast,
    VerySlow,
}
