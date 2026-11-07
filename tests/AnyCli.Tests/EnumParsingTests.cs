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

        var result = await Run(app, ["mode", "very-slow"]);

        await Assert.That(result.Output).IsEqualTo("VerySlow");
    }

    [Test]
    public async Task CustomEnumArgumentParser_OverridesDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<GlobalEnumCommand, GlobalEnumSettings>("mode"))
            .ConfigureParsingDefaults(defaults =>
                defaults.SetEnumArgumentParser<StrictEnumArgumentParser>())
            .Build();
        CommandArgumentException? exception = null;

        var result = await Run(app, ["mode", "VerySlow"]);
        try
        {
            _ = await Run(app, ["mode", "very-slow"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(result.Output).IsEqualTo("VerySlow");
        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("very-slow");
    }

    private static async Task<RunResult> Run(ConsoleApp app, string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output);

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

public sealed class StrictEnumArgumentParser : IEnumArgumentParser
{
    public static bool TryParse<TEnum>(
        string value,
        IFormatProvider? provider,
        out TEnum result)
        where TEnum : struct, Enum => Enum.TryParse(value, ignoreCase: false, out result);
}

public enum KebabMode
{
    Fast,
    VerySlow,
}
