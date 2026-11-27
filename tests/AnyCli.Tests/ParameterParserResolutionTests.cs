// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class ParameterParserResolutionTests
{
    [Test]
    public async Task SyncParser_UsesRegisteredParser()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new ParserPrefix("di:"));
        serviceProvider.Add(() => new ResolvedValueParser(new ParserPrefix("di:")));
        serviceProvider.Add(() => new ResolvedValueCommand());

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<ResolvedValueCommand, ResolvedValueParameters>("parse"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["parse", "value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("di:value");
    }

    [Test]
    public async Task SyncParser_FallsBackToParameterlessConstructor()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new ResolvedValueCommand());

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<ResolvedValueCommand, ResolvedValueParameters>("parse"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["parse", "value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("value");
    }
}

public sealed record ResolvedValue(string Value);

public sealed class ParserPrefix(string value)
{
    public string Value { get; } = value;
}

public sealed class ResolvedValueParser : IParameterParser<ResolvedValue>
{
    private readonly ParserPrefix? _prefix;

    public ResolvedValueParser()
    {
    }

    public ResolvedValueParser(ParserPrefix prefix)
    {
        _prefix = prefix;
    }

    public bool TryParse(string value, IFormatProvider? provider, out ResolvedValue result)
    {
        result = new ResolvedValue((_prefix?.Value ?? string.Empty) + value);
        return true;
    }
}

public sealed partial class ResolvedValueParameters
{
    [Argument(0, Parser = typeof(ResolvedValueParser))]
    public ResolvedValue Value { get; init; } = new(string.Empty);
}

internal sealed class ResolvedValueCommand : ICommand<ResolvedValueParameters>
{
    public int Execute(CommandContext context, ResolvedValueParameters parameters)
    {
        context.Console.Output.Write(parameters.Value.Value);
        return 0;
    }
}
