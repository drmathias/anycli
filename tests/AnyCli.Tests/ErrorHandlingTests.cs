// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.SpectreConsole;
using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class ErrorHandlingTests
{
    [Test]
    public async Task DefaultResources_ContainAllEnglishMessages()
    {
        var resources = new System.Resources.ResourceManager(
            "AnyCli.Errors.CommandErrorMessages",
            typeof(LocalizedCommandErrorMessageProvider).Assembly);
        var english = System.Globalization.CultureInfo.GetCultureInfo("en");

        foreach (var key in new[]
        {
            "NoCommandSpecified",
            "UnknownCommand",
            "MissingSubcommand",
            "UnknownOption",
            "MalformedOption",
            "OptionRequiresSeparateValue",
            "MissingOptionValue",
            "RepeatedOption",
            "MissingRequiredOption",
            "MissingRequiredArgument",
            "UnexpectedArgument",
            "TooFewVariadicValues",
            "TooManyVariadicValues",
            "InvalidParameterValue",
        })
        {
            await Assert.That(resources.GetString(key, english)).IsNotNull();
        }
    }

    [Test]
    public async Task DefaultResources_FallBackToEnglishForUnknownCulture()
    {
        var resources = new System.Resources.ResourceManager(
            "AnyCli.Errors.CommandErrorMessages",
            typeof(LocalizedCommandErrorMessageProvider).Assembly);
        var japanese = System.Globalization.CultureInfo.GetCultureInfo("ja-JP");

        var message = resources.GetString("UnknownOption", japanese);

        await Assert.That(message).IsEqualTo("Unknown option '{0}'.");
    }

    [Test]
    public async Task DefaultResources_ContainTranslations()
    {
        var resources = new System.Resources.ResourceManager(
            "AnyCli.Errors.CommandErrorMessages",
            typeof(LocalizedCommandErrorMessageProvider).Assembly);

        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["fr"] = "Option inconnue '{0}'.",
            ["de"] = "Unbekannte Option '{0}'.",
            ["es"] = "Opción desconocida '{0}'.",
            ["pt-BR"] = "Opção desconhecida '{0}'.",
            ["it"] = "Opzione sconosciuta '{0}'.",
            ["nl"] = "Onbekende optie '{0}'.",
        };

        foreach (var (cultureName, expectedMessage) in expected)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureName);
            var message = resources.GetString("UnknownOption", culture);

            await Assert.That(message).IsEqualTo(expectedMessage);
        }
    }

    [Test]
    public async Task DefaultMessageProvider_UsesCultureSpecificMessage()
    {
        var provider = new FixedCultureMessageProvider(
            System.Globalization.CultureInfo.GetCultureInfo("de"));
        using var error = new StringWriter();
        var console = new TextConsole(new StringReader(string.Empty), new StringWriter(), error);
        var context = new CommandErrorContext(
            "anycli",
            [],
            new UnknownOptionError("--nope"),
            console);

        var message = provider.GetMessage(context);

        await Assert.That(message).IsEqualTo("Unbekannte Option '--nope'.");
    }

    [Test]
    public async Task CustomMessageProvider_OverridesIndividualError()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureErrorHandling(errors => errors
                .UseMessageProvider<CustomMessageProvider>())
            .ConfigureCommands(commands => commands.AddCommand<NoOpCommand>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run", "--nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"custom option: --nope{Environment.NewLine}");
    }

    [Test]
    public async Task CustomErrorWriter_WritesToErrorWriter()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureErrorHandling(errors => errors
                .UseWriter<PrefixErrorWriter>())
            .ConfigureCommands(_ => { })
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).IsEqualTo($"written: Unknown command 'nope'.{Environment.NewLine}");
    }

    [Test]
    public async Task TooFewVariadicValues_IsDistinctStructuredError()
    {
        var handler = new CapturingErrorHandler();
        var app = CreateCapturingApp(
            handler,
            commands => commands.AddCommand<VariadicCommand, ErrorVariadicParameters>("values"));

        var result = await TestAppHarness.RunAsync(app, ["values", "1"]);

        await Assert.That(result.ExitCode).IsEqualTo(5);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(handler.Error is TooFewVariadicValuesError).IsTrue();

        var error = (TooFewVariadicValuesError)handler.Error!;
        await Assert.That(error.ArgumentName).IsEqualTo("values");
        await Assert.That(error.Minimum).IsEqualTo(2);
        await Assert.That(error.Actual).IsEqualTo(1);
    }

    [Test]
    public async Task TooManyVariadicValues_IsDistinctStructuredError()
    {
        var handler = new CapturingErrorHandler();
        var app = CreateCapturingApp(
            handler,
            commands => commands.AddCommand<VariadicCommand, ErrorVariadicParameters>("values"));

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3", "4"]);

        await Assert.That(result.ExitCode).IsEqualTo(5);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(handler.Error is TooManyVariadicValuesError).IsTrue();

        var error = (TooManyVariadicValuesError)handler.Error!;
        await Assert.That(error.ArgumentName).IsEqualTo("values");
        await Assert.That(error.Maximum).IsEqualTo(3);
        await Assert.That(error.Actual).IsEqualTo(4);
    }

    [Test]
    public async Task InvalidArgumentValue_ReportsArgumentSource()
    {
        var handler = new CapturingErrorHandler();
        var app = CreateCapturingApp(
            handler,
            commands => commands.AddCommand<ArgumentCommand, ErrorArgumentParameters>("parse"));

        var result = await TestAppHarness.RunAsync(app, ["parse", "nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(5);
        await Assert.That(handler.Error is InvalidParameterValueError).IsTrue();

        var error = (InvalidParameterValueError)handler.Error!;
        await Assert.That(error.ParameterName).IsEqualTo("count");
        await Assert.That(error.Value).IsEqualTo("nope");
        await Assert.That(error.Source).IsEqualTo(ParameterSource.Argument);
    }

    [Test]
    public async Task InvalidOptionValue_ReportsOptionSource()
    {
        var handler = new CapturingErrorHandler();
        var app = CreateCapturingApp(
            handler,
            commands => commands.AddCommand<OptionCommand, ErrorOptionParameters>("parse"));

        var result = await TestAppHarness.RunAsync(app, ["parse", "--count", "nope"]);

        await Assert.That(result.ExitCode).IsEqualTo(5);
        await Assert.That(handler.Error is InvalidParameterValueError).IsTrue();

        var error = (InvalidParameterValueError)handler.Error!;
        await Assert.That(error.ParameterName).IsEqualTo("--count");
        await Assert.That(error.Value).IsEqualTo("nope");
        await Assert.That(error.Source).IsEqualTo(ParameterSource.Option);
    }

    [Test]
    public async Task SpectreErrorWriter_UsesAnsiWhenForced()
    {
        using var error = new StringWriter();
        var console = new TextConsole(new StringReader(string.Empty), new StringWriter(), error);
        var context = new CommandErrorContext(
            "anycli",
            [],
            new UnknownCommandError("nope", []),
            console);
        var writer = new SpectreCommandErrorWriter(SpectreConsoleOptions.ForTesting);

        await writer.WriteAsync(context, "Unknown command 'nope'.", CancellationToken.None);

        await Assert.That(error.ToString()).Contains("\u001b[");
        await Assert.That(error.ToString()).Contains("Unknown command");
    }

    private static ConsoleApp CreateCapturingApp(
        CapturingErrorHandler handler,
        Action<CommandRegistrar> configureCommands)
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => handler);

        return ConsoleAppBuilder.Create()
            .WithName("anycli")
            .UseServiceProvider(serviceProvider)
            .ConfigureErrorHandling(errors => errors
                .UseHandler<CapturingErrorHandler>()
                .UseExitCode(5))
            .ConfigureCommands(configureCommands)
            .Build();
    }

    private sealed class FixedCultureMessageProvider(
        System.Globalization.CultureInfo culture) : LocalizedCommandErrorMessageProvider
    {
        protected override System.Globalization.CultureInfo GetUICulture(
            CommandErrorContext context) =>
            culture;
    }

    private sealed class CustomMessageProvider : LocalizedCommandErrorMessageProvider
    {
        protected override string GetMessage(
            CommandErrorContext context,
            UnknownOptionError error) =>
            $"custom option: {error.OptionName}";
    }

    private sealed class PrefixErrorWriter : ICommandErrorWriter
    {
        public ValueTask WriteAsync(
            CommandErrorContext context,
            string message,
            CancellationToken cancellationToken)
        {
            context.Console.Error.WriteLine($"written: {message}");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CapturingErrorHandler : ICommandErrorHandler
    {
        public CommandError? Error { get; private set; }

        public ValueTask HandleAsync(
            CommandErrorContext context,
            CancellationToken cancellationToken)
        {
            Error = context.Error;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class NoOpCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context) => 0;
    }

    private sealed class VariadicCommand : ICommand<ErrorVariadicParameters>
    {
        public int Execute(CommandContext context, ErrorVariadicParameters parameters) => 0;
    }

    private sealed class ArgumentCommand : ICommand<ErrorArgumentParameters>
    {
        public int Execute(CommandContext context, ErrorArgumentParameters parameters) => 0;
    }

    private sealed class OptionCommand : ICommand<ErrorOptionParameters>
    {
        public int Execute(CommandContext context, ErrorOptionParameters parameters) => 0;
    }
}

public sealed partial class ErrorVariadicParameters
{
    [VariadicArgument(0, MinimumCount = 2, MaximumCount = 3)]
    public int[] Values { get; set; } = [];
}

public sealed partial class ErrorArgumentParameters
{
    [Argument(0)]
    public int Count { get; set; }
}

public sealed partial class ErrorOptionParameters
{
    [Option("--count")]
    public int Count { get; set; }
}
