// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

using AnyCli.Tests.Fixtures;
using AnyCli.Tests.Helpers;

namespace AnyCli.Tests;

public class DefaultValueTests
{
    [Test]
    public async Task OptionDefaults_AreTypedAndApplied()
    {
        var app = CreateOptionsApp();

        var result = await TestAppHarness.RunAsync(app, ["opts"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Text|True|3|default");
    }

    [Test]
    public async Task ExplicitValues_OverrideOptionDefaults()
    {
        var app = CreateOptionsApp();

        var result = await TestAppHarness.RunAsync(
            app,
            ["opts", "--mode", "json", "--verbose=false", "--count", "5", "--name", "cli"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Json|False|5|cli");
    }

    [Test]
    public async Task OptionalArgumentDefaults_AreTypedAndApplied()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultArgumentCommand, DefaultArgumentParameters>("arg"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["arg"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("42|Json");
    }

    [Test]
    public async Task ExplicitValues_OverrideArgumentDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultArgumentCommand, DefaultArgumentParameters>("arg"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["arg", "7", "text"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("7|Text");
    }

    [Test]
    public async Task CollectionOptionDefaults_AreTypedAndApplied()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<CollectionDefaultsCommand, CollectionDefaultsParameters>("opts"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["opts"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("latest,stable|Text,Json");
    }

    [Test]
    public async Task ExplicitValues_OverrideCollectionOptionDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<CollectionDefaultsCommand, CollectionDefaultsParameters>("opts"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["opts", "--tag", "custom", "--mode", "json"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("custom|Json");
    }

    [Test]
    public async Task VariadicDefaults_AreTypedAndApplied()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<VariadicDefaultsCommand, VariadicDefaultsParameters>("values"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["values"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("a,b");
    }

    [Test]
    public async Task ExplicitValues_OverrideVariadicDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<VariadicDefaultsCommand, VariadicDefaultsParameters>("values"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["values", "x", "y"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("x,y");
    }

    [Test]
    public async Task DefaultValueProviders_AreUsedForOmittedParameters()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<ProviderCommand, ProviderParameters>("provider"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["provider"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("provider|7|a,b");
    }

    [Test]
    public async Task DefaultValueProviders_YieldToSuppliedValues()
    {
        var provider = new CountingStringDefaultProvider("provider");
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => provider);
        serviceProvider.Add(() => new SuppliedValueCommand());

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<SuppliedValueCommand, SuppliedValueParameters>("value"))
            .Build();

        var supplied = await TestAppHarness.RunAsync(app, ["value", "--name", "cli"]);
        var omitted = await TestAppHarness.RunAsync(app, ["value"]);

        await Assert.That(supplied.Output).IsEqualTo("cli");
        await Assert.That(omitted.Output).IsEqualTo("provider");
        await Assert.That(provider.Invocations).IsEqualTo(1);
    }

    [Test]
    public async Task DefaultValueProvider_UsesRegisteredProviderWhenParameterlessConstructorExists()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new ConfigurableProvider(new ProviderPrefix("di:")));
        serviceProvider.Add(() => new ConfigurableProviderCommand());

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<ConfigurableProviderCommand, ConfigurableProviderParameters>("value"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("di:default");
    }

    [Test]
    public async Task DefaultValueProvider_FallsBackToParameterlessConstructor()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new ConfigurableProviderCommand());

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<ConfigurableProviderCommand, ConfigurableProviderParameters>("value"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("parameterless:default");
    }

    [Test]
    public async Task AsyncDefaultValueProvider_FallsBackToParameterlessConstructor()
    {
        var serviceProvider = new StubServiceProvider();
        serviceProvider.Add(() => new AsyncProviderCommand());

        var app = ConsoleAppBuilder.Create()
            .UseServiceProvider(serviceProvider)
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncProviderCommand, AsyncProviderParameters>("value"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("async|async|async-tag");
    }

    [Test]
    public async Task VariadicProviderDefault_IgnoresMinimumAndMaximum()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<ProvidedVariadicCommand, ProvidedVariadicParameters>("values"))
            .Build();

        var omitted = await TestAppHarness.RunAsync(app, ["values"]);
        var tooFew = await TestAppHarness.RunAsync(app, ["values", "one"]);
        var tooMany = await TestAppHarness.RunAsync(app, ["values", "one", "two", "three", "four"]);
        var supplied = await TestAppHarness.RunAsync(app, ["values", "one", "two"]);

        await Assert.That(omitted.Output).IsEqualTo("a,b,c,d");
        await Assert.That(tooFew.ExitCode).IsEqualTo(2);
        await Assert.That(tooMany.ExitCode).IsEqualTo(2);
        await Assert.That(supplied.Output).IsEqualTo("one,two");
    }

    [Test]
    public async Task VariadicDefault_MakesRequiredArityOptionalInHelp()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureCommands(commands =>
                commands.AddCommand<ProvidedVariadicCommand, ProvidedVariadicParameters>("values"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["values", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("[values...]");
        await Assert.That(result.Output).DoesNotContain("<values...>");
    }

    [Test]
    public async Task VariadicInitializerDefault_IgnoresMinimumAndMaximum()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<InitializedVariadicCommand, InitializedVariadicParameters>("values"))
            .Build();

        var omitted = await TestAppHarness.RunAsync(app, ["values"]);
        var tooFew = await TestAppHarness.RunAsync(app, ["values", "one"]);
        var supplied = await TestAppHarness.RunAsync(app, ["values", "one", "two"]);

        await Assert.That(omitted.Output).IsEqualTo("a,b,c,d");
        await Assert.That(tooFew.ExitCode).IsEqualTo(2);
        await Assert.That(supplied.Output).IsEqualTo("one,two");
    }

    [Test]
    public async Task AsyncDefaultValueProvider_IsAwaited()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<AsyncProviderCommand, AsyncProviderParameters>("value"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["value"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("async|async|async-tag");
    }

    [Test]
    public async Task DefaultHelp_ShowsDefaultValueDisplays()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureHelp(help => help
                .UseLocalizedTextProvider(Resources.HelpDescriptions.ResourceManager))
            .ConfigureCommands(commands =>
                commands.AddCommand<ProviderCommand, ProviderParameters>("provider"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["provider", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("[default: provider]");
        await Assert.That(result.Output).Contains("[default: Number of items]");
        await Assert.That(result.Output).Contains("[default: a,b]");
    }

    [Test]
    public async Task DefaultHelp_LocalizesDefaultValueDisplays()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("es"))
            .ConfigureHelp(help => help
                .UseLocalizedTextProvider(Resources.HelpDescriptions.ResourceManager))
            .ConfigureCommands(commands =>
                commands.AddCommand<ProviderCommand, ProviderParameters>("provider"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["provider", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("[predeterminado: Número de elementos]");
    }

    [Test]
    public async Task PropertyInitializer_RemainsWhenNoDefaultIsConfigured()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<InitializerCommand, InitializerParameters>("init"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["init"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("initializer");
    }

    [Test]
    public async Task DefaultHelp_ShowsCapturedCollectionInitializerDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureCommands(commands =>
                commands.AddCommand<CollectionDefaultsCommand, CollectionDefaultsParameters>("opts"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["opts", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("[default: latest, stable]");
        await Assert.That(result.Output).Contains("[default: text, json]");
    }

    [Test]
    public async Task DefaultHelp_ShowsInitializerFallback()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureCommands(commands =>
                commands.AddCommand<InitializerCommand, InitializerParameters>("init"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["init", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("[default: initializer]");
    }

    [Test]
    public async Task CustomHelpValueFormatter_FormatsEnumDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureLocalization(LocalizationOptions.FromCulture("en"))
            .ConfigureHelp(help => help
                .UseValueFormatter<UpperCaseHelpValueFormatter>())
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultOptionsCommand, DefaultOptionsParameters>("opts"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["opts", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("[default: TEXT]");
    }

    [Test]
    public async Task CustomHelpProvider_ReceivesConfiguredValueFormatter()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureHelp(help => help
                .UseValueFormatter<UpperCaseHelpValueFormatter>()
                .UseProvider(context => new FormatterHelpProvider(context.ValueFormatter)))
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultOptionsCommand, DefaultOptionsParameters>("opts"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("TEXT");
    }

    [Test]
    public async Task HelpMetadata_ExposesTypedDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultOptionsCommand, DefaultOptionsParameters>("opts"))
            .ConfigureHelp(help => help.UseProvider<DefaultHelpProvider>())
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["opts", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("--mode:AnyCli.Tests.DefaultOutputMode:Text");
        await Assert.That(result.Output).Contains("--verbose:System.Boolean:True");
        await Assert.That(result.Output).Contains("--count:System.Int32:3");
        await Assert.That(result.Output).Contains("--name:System.String:default");
    }

    private sealed class ProviderCommand : ICommand<ProviderParameters>
    {
        public int Execute(CommandContext context, ProviderParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.Count}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }

    private sealed class SuppliedValueCommand : ICommand<SuppliedValueParameters>
    {
        public int Execute(CommandContext context, SuppliedValueParameters parameters)
        {
            context.Console.Output.Write(parameters.Name);
            return 0;
        }
    }

    private sealed class ConfigurableProviderCommand : ICommand<ConfigurableProviderParameters>
    {
        public int Execute(CommandContext context, ConfigurableProviderParameters parameters)
        {
            context.Console.Output.Write(parameters.Name);
            return 0;
        }
    }

    private sealed class AsyncProviderCommand : ICommand<AsyncProviderParameters>
    {
        public int Execute(CommandContext context, AsyncProviderParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.Label}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }

    private sealed class ProvidedVariadicCommand : ICommand<ProvidedVariadicParameters>
    {
        public int Execute(CommandContext context, ProvidedVariadicParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class InitializedVariadicCommand : ICommand<InitializedVariadicParameters>
    {
        public int Execute(CommandContext context, InitializedVariadicParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private static ConsoleApp CreateOptionsApp() =>
        ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultOptionsCommand, DefaultOptionsParameters>("opts"))
            .Build();

    private sealed class DefaultOptionsCommand : ICommand<DefaultOptionsParameters>
    {
        public int Execute(CommandContext context, DefaultOptionsParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Mode}|{parameters.Verbose}|{parameters.Count}|{parameters.Name}");
            return 0;
        }
    }

    private sealed class DefaultArgumentCommand : ICommand<DefaultArgumentParameters>
    {
        public int Execute(CommandContext context, DefaultArgumentParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Count}|{parameters.Mode}");
            return 0;
        }
    }

    private sealed class CollectionDefaultsCommand : ICommand<CollectionDefaultsParameters>
    {
        public int Execute(CommandContext context, CollectionDefaultsParameters parameters)
        {
            context.Console.Output.Write(
                $"{string.Join(",", parameters.Tags)}|{string.Join(",", parameters.Modes)}");
            return 0;
        }
    }

    private sealed class VariadicDefaultsCommand : ICommand<VariadicDefaultsParameters>
    {
        public int Execute(CommandContext context, VariadicDefaultsParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class InitializerCommand : ICommand<InitializerParameters>
    {
        public int Execute(CommandContext context, InitializerParameters parameters)
        {
            context.Console.Output.Write(parameters.Name);
            return 0;
        }
    }

    private sealed class FormatterHelpProvider(IHelpValueFormatter formatter) : IHelpProvider
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            context.Console.Output.Write(
                formatter.Format(DefaultOutputMode.Text, CultureInfo.InvariantCulture) ?? "null");
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            context.Console.Output.Write(
                formatter.Format(DefaultOutputMode.Text, CultureInfo.InvariantCulture) ?? "null");
            return ValueTask.CompletedTask;
        }
    }

    private sealed class DefaultHelpProvider : IHelpProvider
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write("root");
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write(string.Join(
                "\n",
                context.Command.Options.Select(option =>
                {
                    var value = option.DefaultValue switch
                    {
                        DefaultValueInfo.Initializer initializer =>
                            $"{initializer.Value?.GetType().FullName}:{initializer.Value}",
                        DefaultValueInfo.Provider provider =>
                            $"provider:{provider.ProviderType.FullName}",
                        _ => "none",
                    };

                    return $"{option.Names[0]}:{value}";
                })));
            return ValueTask.CompletedTask;
        }
    }
}

public sealed partial class DefaultOptionsParameters
{
    [Option("mode")]
    public DefaultOutputMode Mode { get; set; } = DefaultOutputMode.Text;

    [Option("verbose")]
    public bool Verbose { get; set; } = true;

    [Option("count")]
    public int Count { get; set; } = 3;

    [Option("name")]
    public string Name { get; set; } = "default";
}

public sealed partial class DefaultArgumentParameters
{
    [OptionalArgument(0)]
    public int Count { get; set; } = 42;

    [OptionalArgument(1)]
    public DefaultOutputMode Mode { get; set; } = DefaultOutputMode.Json;
}

public sealed partial class CollectionDefaultsParameters
{
    [Option("tag")]
    public string[] Tags { get; set; } = ["latest", "stable"];

    [Option("mode")]
    public DefaultOutputMode[] Modes { get; set; } = [DefaultOutputMode.Text, DefaultOutputMode.Json];
}

public sealed partial class VariadicDefaultsParameters
{
    [VariadicArgument(0)]
    public string[] Values { get; set; } = ["a", "b"];
}

public sealed partial class InitializerParameters
{
    [Option("name")]
    public string Name { get; set; } = "initializer";
}

public enum DefaultOutputMode
{
    Text,
    Json,
}

public sealed partial class ProviderParameters
{
    [Option("--name", DefaultValueProvider = typeof(ProviderNameDefault))]
    [DefaultValueDisplay("provider")]
    public string Name { get; set; } = "initializer";

    [OptionalArgument(0, DefaultValueProvider = typeof(ProviderCountDefault))]
    [DefaultValueDisplayResource("CountDescription")]
    public int Count { get; set; } = 1;

    [VariadicArgument(1, DefaultValueProvider = typeof(ProviderTagsDefault))]
    [DefaultValueDisplay("a,b")]
    public string[] Tags { get; set; } = [];
}

public sealed partial class ConfigurableProviderParameters
{
    [Option("--name", DefaultValueProvider = typeof(ConfigurableProvider))]
    public string Name { get; set; } = "";
}

public sealed class ProviderPrefix(string value)
{
    public string Value { get; } = value;
}

public sealed class ConfigurableProvider : IDefaultValueProvider<string>
{
    private readonly ProviderPrefix? _prefix;

    public ConfigurableProvider()
    {
    }

    public ConfigurableProvider(ProviderPrefix prefix)
    {
        _prefix = prefix;
    }

    public string CreateDefault() => (_prefix?.Value ?? "parameterless:") + "default";
}

public sealed partial class SuppliedValueParameters
{
    [Option("--name", DefaultValueProvider = typeof(CountingStringDefaultProvider))]
    public string Name { get; set; } = "";
}

public sealed class ProviderNameDefault : IDefaultValueProvider<string>
{
    public string CreateDefault() => "provider";
}

public sealed class ProviderCountDefault : IDefaultValueProvider<int>
{
    public int CreateDefault() => 7;
}

public sealed class ProviderTagsDefault : IDefaultValueProvider<string[]>
{
    public string[] CreateDefault() => ["a", "b"];
}

public sealed class CountingStringDefaultProvider(string value) : IDefaultValueProvider<string>
{
    public int Invocations { get; private set; }

    public string CreateDefault()
    {
        Invocations++;
        return value;
    }
}


public sealed partial class AsyncProviderParameters
{
    [Option("--name", DefaultValueProvider = typeof(AsyncNameDefaultProvider))]
    public string Name { get; set; } = "";

    [OptionalArgument(0, DefaultValueProvider = typeof(AsyncNameDefaultProvider))]
    public string Label { get; set; } = "";

    [VariadicArgument(1, DefaultValueProvider = typeof(AsyncTagsDefaultProvider))]
    public string[] Tags { get; set; } = [];
}

public sealed class AsyncNameDefaultProvider : IAsyncDefaultValueProvider<string>
{
    public async ValueTask<string> CreateDefaultAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        return "async";
    }
}

public sealed class AsyncTagsDefaultProvider : IAsyncDefaultValueProvider<string[]>
{
    public async ValueTask<string[]> CreateDefaultAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        return ["async-tag"];
    }
}


public sealed partial class ProvidedVariadicParameters
{
    [VariadicArgument(0, MinimumCount = 2, MaximumCount = 3,
        DefaultValueProvider = typeof(VariadicItemsDefaultProvider))]
    public string[] Values { get; set; } = [];
}

public sealed partial class InitializedVariadicParameters
{
    [VariadicArgument(0, MinimumCount = 2, MaximumCount = 3)]
    public string[] Values { get; set; } = ["a", "b", "c", "d"];
}

public sealed class VariadicItemsDefaultProvider : IDefaultValueProvider<string[]>
{
    public string[] CreateDefault() => ["a", "b", "c", "d"];
}


public sealed class UpperCaseHelpValueFormatter : IHelpValueFormatter
{
    public string? Format(object? value, CultureInfo culture) =>
        value is Enum enumValue
            ? enumValue.ToString().ToUpperInvariant()
            : Convert.ToString(value, culture);
}
