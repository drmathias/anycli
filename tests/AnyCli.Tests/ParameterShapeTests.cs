// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class ParameterShapeTests
{
    [Test]
    public async Task ImmutableRecordParameters_BindArgumentsOptionsAndCollections()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands
                .AddCommand<ImmutableRecordCommand, ImmutableRecordParameters>("record"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["record", "Ada", "--greeting", "Hi", "--tag", "one", "--tag", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Hi, Ada!|one,two");
    }

    [Test]
    public async Task ImmutableRecordParameters_BindOptionalArgumentsVariadicArgumentsAndDefaults()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands
                .AddCommand<ImmutableArgumentsCommand, ImmutableArgumentsParameters>("arguments"))
            .Build();

        var defaults = await TestAppHarness.RunAsync(app, ["arguments", "Ada"]);
        var supplied = await TestAppHarness.RunAsync(
            app,
            ["arguments", "Ada", "custom", "one", "two"]);

        await Assert.That(defaults.Output).IsEqualTo("Ada|note|");
        await Assert.That(supplied.Output).IsEqualTo("Ada|custom|one,two");
    }

    [Test]
    public async Task ImmutableRecordGlobalOptions_Bind()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddGlobalOptions<ImmutableGlobalOptions>()
            .ConfigureCommands(commands => commands
                .AddCommand<ImmutableGlobalCommand, ImmutableGlobalParameters>("global"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["--data", "other.json", "--quiet", "global", "Ada"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Ada|other.json|True");
    }


    [Test]
    public async Task ClassInitOnlyParameters_Bind()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands
                .AddCommand<ClassInitOnlyCommand, ClassInitOnlyParameters>("class"))
            .Build();

        var defaults = await TestAppHarness.RunAsync(app, ["class", "Ada"]);
        var supplied = await TestAppHarness.RunAsync(app, ["class", "Ada", "--count", "7"]);

        await Assert.That(defaults.Output).IsEqualTo("Ada|3");
        await Assert.That(supplied.Output).IsEqualTo("Ada|7");
    }

    [Test]
    public async Task ClassRequiredParameters_Bind()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands
                .AddCommand<ClassRequiredCommand, ClassRequiredParameters>("required"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["required", "Ada", "--greeting", "Hi", "--tag", "one", "--tag", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Ada|Hi|one,two");
    }

    [Test]
    public async Task RecordRequiredParameters_Bind()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands
                .AddCommand<RecordRequiredCommand, RecordRequiredParameters>("required-record"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["required-record", "Ada", "--tag", "one", "--tag", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Ada|one,two");
    }

    [Test]
    public async Task MutableRecordParameters_Bind()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands
                .AddCommand<MutableRecordCommand, MutableRecordParameters>("mutable-record"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["mutable-record", "Ada", "--tag", "one", "--tag", "two"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Ada|one,two");
    }

    [Test]
    public async Task ClassInitOnlyGlobalOptions_Bind()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .AddGlobalOptions<ClassGlobalOptions>()
            .ConfigureCommands(commands => commands
                .AddCommand<ClassGlobalCommand, ClassGlobalParameters>("global-class"))
            .Build();

        var result = await TestAppHarness.RunAsync(
            app,
            ["--data", "other.json", "global-class", "Ada"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("Ada|other.json");
    }

    private sealed class ImmutableRecordCommand : ICommand<ImmutableRecordParameters>
    {
        public int Execute(CommandContext context, ImmutableRecordParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Greeting}, {parameters.Name}!|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }


    private sealed class ClassInitOnlyCommand : ICommand<ClassInitOnlyParameters>
    {
        public int Execute(CommandContext context, ClassInitOnlyParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Name}|{parameters.Count}");
            return 0;
        }
    }

    private sealed class ClassRequiredCommand : ICommand<ClassRequiredParameters>
    {
        public int Execute(CommandContext context, ClassRequiredParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.Greeting}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }

    private sealed class RecordRequiredCommand : ICommand<RecordRequiredParameters>
    {
        public int Execute(CommandContext context, RecordRequiredParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Name}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }

    private sealed class MutableRecordCommand : ICommand<MutableRecordParameters>
    {
        public int Execute(CommandContext context, MutableRecordParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Name}|{string.Join(",", parameters.Tags)}");
            return 0;
        }
    }

    private sealed class ClassGlobalCommand : ICommand<ClassGlobalParameters>
    {
        public int Execute(CommandContext context, ClassGlobalParameters parameters)
        {
            context.Console.Output.Write($"{parameters.Name}|{parameters.DataFile}");
            return 0;
        }
    }

    private sealed class ImmutableArgumentsCommand : ICommand<ImmutableArgumentsParameters>
    {
        public int Execute(CommandContext context, ImmutableArgumentsParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.Note}|{string.Join(",", parameters.Values)}");
            return 0;
        }
    }

    private sealed class ImmutableGlobalCommand : ICommand<ImmutableGlobalParameters>
    {
        public int Execute(CommandContext context, ImmutableGlobalParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Name}|{parameters.DataFile}|{parameters.Quiet}");
            return 0;
        }
    }
}

public sealed partial record ImmutableRecordParameters
{
    [Argument(0)]
    public string Name { get; init; } = "";

    [Option("--greeting")]
    public string Greeting { get; init; } = "Hello";

    [Option("--tag")]
    public string[] Tags { get; init; } = [];
}

public sealed partial record ImmutableArgumentsParameters
{
    [Argument(0)]
    public string Name { get; init; } = "";

    [OptionalArgument(1)]
    public string Note { get; init; } = "note";

    [VariadicArgument(2)]
    public string[] Values { get; init; } = [];
}

[GlobalOptions]
public partial record ImmutableGlobalOptions
{
    [Option("--data")]
    public string DataFile { get; init; } = "data.json";

    [Option("--quiet")]
    public bool Quiet { get; init; }
}

public sealed partial record ImmutableGlobalParameters : ImmutableGlobalOptions
{
    [Argument(0)]
    public string Name { get; init; } = "";
}

public sealed partial class ClassInitOnlyParameters
{
    [Argument(0)]
    public string Name { get; init; } = "";

    [Option("--count")]
    public int Count { get; init; } = 3;
}

public sealed partial class ClassRequiredParameters
{
    [Argument(0)]
    public required string Name { get; set; }

    [Option("--greeting")]
    public required string Greeting { get; init; }

    [Option("--tag")]
    public required string[] Tags { get; init; }
}

public sealed partial record RecordRequiredParameters
{
    [Argument(0)]
    public required string Name { get; init; }

    [Option("--tag")]
    public required string[] Tags { get; init; }
}

public sealed partial record MutableRecordParameters
{
    [Argument(0)]
    public string Name { get; set; } = "";

    [Option("--tag")]
    public string[] Tags { get; set; } = [];
}

[GlobalOptions]
public partial class ClassGlobalOptions
{
    [Option("--data")]
    public string DataFile { get; init; } = "data.json";
}

public sealed partial class ClassGlobalParameters : ClassGlobalOptions
{
    [Argument(0)]
    public string Name { get; init; } = "";
}
