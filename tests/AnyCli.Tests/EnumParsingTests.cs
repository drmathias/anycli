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
        var success = DefaultEnumParameterParser.TryParse<FileAccessMode>(
            "read-only",
            out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(FileAccessMode.ReadOnly);
    }

    [Test]
    public async Task DefaultEnumParameterParser_RejectsMalformedKebabCase()
    {
        foreach (var value in new[] { "r-e-a-d-o-n-l-y", "read--only", "-read-only", "read-only-" })
        {
            var success = DefaultEnumParameterParser.TryParse<FileAccessMode>(
                value,
                out _);

            await Assert.That(success).IsFalse();
        }
    }

    [Test]
    public async Task DefaultEnumParameterParser_UsesKebabCaseNamingPolicy()
    {
        var success = DefaultEnumParameterParser.TryParse<AcronymMode>(
            "xml-http-request",
            out var result);

        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(AcronymMode.XMLHttpRequest);
    }

    [Test]
    public async Task DefaultEnumParameterParser_RejectsNumericValues()
    {
        foreach (var value in new[] { "0", "1", "-1", "+1", "0x1", "999", "1,2" })
        {
            var success = DefaultEnumParameterParser.TryParse<FileAccessMode>(
                value,
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
            _ = ParameterValueParsing.ParseEnum<FileAccessMode>("1", "mode");
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
        var result = ParameterValueParsing.ParseEnum<FileAccessMode>("ReadOnly", "mode");

        await Assert.That(result).IsEqualTo(FileAccessMode.ReadOnly);
    }

    [Test]
    public async Task ParseEnum_MatchesLowercaseNames()
    {
        var result = ParameterValueParsing.ParseEnum<FileAccessMode>("readonly", "mode");

        await Assert.That(result).IsEqualTo(FileAccessMode.ReadOnly);
    }

    [Test]
    public async Task ParseEnum_MatchesKebabCase()
    {
        var result = ParameterValueParsing.ParseEnum<FileAccessMode>("read-only", "mode");

        await Assert.That(result).IsEqualTo(FileAccessMode.ReadOnly);
    }

    [Test]
    public async Task ParseEnum_MatchesMixedCaseKebabValues()
    {
        var result = ParameterValueParsing.ParseEnum<FileAccessMode>("Read-Only", "mode");

        await Assert.That(result).IsEqualTo(FileAccessMode.ReadOnly);
    }

    [Test]
    public async Task GeneratedParameters_UseDefaultEnumParsing()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumParameters>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "read-only"]);

        await Assert.That(result.Output).IsEqualTo("ReadOnly");
    }

    [Test]
    public async Task AssemblyEnumParameterParser_OverridesDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumParameters>("mode"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["mode", "custom"]);

        await Assert.That(result.Output).IsEqualTo("ReadOnly");
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
    public FileAccessMode Mode { get; set; }
}

public sealed class TestEnumParameterParser : IAsyncEnumParameterParser
{
    public ValueTask<ParameterParseResult<TEnum>> TryParseAsync<TEnum>(
        string value,
        IFormatProvider? provider,
        CancellationToken cancellationToken)
        where TEnum : struct, Enum
    {
        cancellationToken.ThrowIfCancellationRequested();

        return typeof(TEnum) == typeof(FileAccessMode) && value == "custom"
            ? ValueTask.FromResult(ParameterParseResult<TEnum>.Succeeded(
                (TEnum)(object)FileAccessMode.ReadOnly))
            : ValueTask.FromResult(
                DefaultEnumParameterParser.TryParse<TEnum>(value, out var result)
                    ? ParameterParseResult<TEnum>.Succeeded(result)
                    : ParameterParseResult<TEnum>.Failed);
    }
}

public enum FileAccessMode
{
    ReadOnly,
    ReadWrite,
}

public enum AcronymMode
{
    XMLHttpRequest,
}
