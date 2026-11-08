// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class InheritedParameterTests
{
    [Test]
    public async Task InheritedParameters_BindInheritedArgumentsAndOptions()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<InheritedParametersCommand, InheritedParameters>("inspect"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["inspect", "source.txt", "--mode", "fast", "--tag", "custom"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|3|fast|custom");
    }

    [Test]
    public async Task InheritedParameters_ApplyInheritedDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<InheritedParametersCommand, InheritedParameters>("inspect"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["inspect", "source.txt", "--mode", "slow"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|3|slow|base");
    }

    [Test]
    public async Task InheritedParameters_CombineInheritedAndDeclaredMembers()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<InheritedExtendedParametersCommand, InheritedExtendedParameters>("inspect"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["inspect", "source.txt", "--mode", "fast", "--extra", "child"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|fast|child|base");
    }

    [Test]
    public async Task InheritedParameters_InheritAttributesThroughOverrides()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands =>
                commands.AddCommand<OverriddenInheritedParametersCommand, OverriddenInheritedParameters>("override"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["override", "source.txt", "--mode", "fast"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|fast");
    }

    private sealed class InheritedParametersCommand : ICommand<InheritedParameters>
    {
        public int Execute(CommandContext context, InheritedParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.Count}|{parameters.Mode}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }

    private sealed class OverriddenInheritedParametersCommand : ICommand<OverriddenInheritedParameters>
    {
        public int Execute(CommandContext context, OverriddenInheritedParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Name}|{parameters.Mode}");
            return 0;
        }
    }

    private sealed class InheritedExtendedParametersCommand : ICommand<InheritedExtendedParameters>
    {
        public int Execute(CommandContext context, InheritedExtendedParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.Mode}|{parameters.Extra ?? "<null>"}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }
}

public class InheritedBaseParameters
{
    [Argument(0)]
    public string Name { get; set; } = "";

    [OptionalArgument(1, DefaultValue = 3)]
    public int Count { get; set; }

    [RequiredOption("--mode")]
    public string Mode { get; set; } = "";

    [Option("--tag", DefaultValue = new[] { "base" })]
    public string[] Tags { get; set; } = [];
}

public sealed partial class InheritedParameters : InheritedBaseParameters
{
}

public class OverriddenInheritedBaseParameters
{
    [Argument(0)]
    public virtual string Name { get; set; } = "";

    [RequiredOption("--mode")]
    public virtual string Mode { get; set; } = "";
}

public sealed partial class OverriddenInheritedParameters : OverriddenInheritedBaseParameters
{
    public override string Name { get; set; } = "";

    public override string Mode { get; set; } = "";
}

public sealed partial class InheritedExtendedParameters : InheritedBaseParameters
{
    [Option("--extra")]
    public string? Extra { get; set; }
}
