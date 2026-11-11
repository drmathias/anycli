// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class DefaultValueTests
{
    [Test]
    public async Task OptionDefaults_AreTypedAndApplied()
    {
        var app = CreateOptionsApp();

        var result = await TestAppHarness.RunAsync(app, ["opts"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Fast|True|3|default");
    }

    [Test]
    public async Task ExplicitValues_OverrideOptionDefaults()
    {
        var app = CreateOptionsApp();

        var result = await TestAppHarness.RunAsync(
            app,
            ["opts", "--mode", "slow", "--verbose=false", "--count", "5", "--name", "cli"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Slow|False|5|cli");
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
        await Assert.That(result.Output).IsEqualTo("42|Slow");
    }

    [Test]
    public async Task ExplicitValues_OverrideArgumentDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultArgumentCommand, DefaultArgumentParameters>("arg"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["arg", "7", "fast"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("7|Fast");
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
        await Assert.That(result.Output).IsEqualTo("latest,stable|Fast,Slow");
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
            ["opts", "--tag", "custom", "--mode", "slow"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("custom|Slow");
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
    public async Task HelpMetadata_ExposesTypedDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .ConfigureCommands(commands =>
                commands.AddCommand<DefaultOptionsCommand, DefaultOptionsParameters>("opts"))
            .AddHelp<DefaultHelpProvider>()
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["opts", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("--mode:AnyCli.Tests.DotNetDefaultMode:Fast");
        await Assert.That(result.Output).Contains("--verbose:System.Boolean:True");
        await Assert.That(result.Output).Contains("--count:System.Int32:3");
        await Assert.That(result.Output).Contains("--name:System.String:default");
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
                    $"{option.Names[0]}:{option.DefaultValue?.GetType().FullName}:{option.DefaultValue}")));
            return ValueTask.CompletedTask;
        }
    }
}

public sealed partial class DefaultOptionsParameters
{
    [Option("mode", DefaultValue = DotNetDefaultMode.Fast)]
    public DotNetDefaultMode Mode { get; set; }

    [Option("verbose", DefaultValue = true)]
    public bool Verbose { get; set; }

    [Option("count", DefaultValue = 3)]
    public int Count { get; set; }

    [Option("name", DefaultValue = "default")]
    public string Name { get; set; } = "initializer";
}

public sealed partial class DefaultArgumentParameters
{
    [OptionalArgument(0, DefaultValue = 42)]
    public int Count { get; set; }

    [OptionalArgument(1, DefaultValue = DotNetDefaultMode.Slow)]
    public DotNetDefaultMode Mode { get; set; }
}

public sealed partial class CollectionDefaultsParameters
{
    [Option("tag", DefaultValue = new[] { "latest", "stable" })]
    public string[] Tags { get; set; } = [];

    [Option("mode", DefaultValue = new[] { DotNetDefaultMode.Fast, DotNetDefaultMode.Slow })]
    public DotNetDefaultMode[] Modes { get; set; } = [];
}

public sealed partial class VariadicDefaultsParameters
{
    [VariadicArgument(0, DefaultValue = new[] { "a", "b" })]
    public string[] Values { get; set; } = [];
}

public sealed partial class InitializerParameters
{
    [Option("name")]
    public string Name { get; set; } = "initializer";
}

public enum DotNetDefaultMode
{
    Fast,
    Slow,
}
