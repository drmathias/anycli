// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class TypedParametersTests
{
    [Test]
    public async Task TypedParameters_BindRequiredAndOptionalArguments()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<CopyCommand, CopyParameters>("copy", "Copies files"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["copy", "source.txt"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|<null>|default|");
    }

    [Test]
    public async Task TypedParameters_BindVariadicArguments()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<CopyCommand, CopyParameters>("copy", "Copies files"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["copy", "source.txt", "dest.txt", "fast", "a", "b"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|dest.txt|fast|a,b");
    }

    [Test]
    public async Task TypedParameters_UseComputedPropertyInitializerAsDefault()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<ComputedCommand, ComputedParameters>("computed"))
            .Build();

        var missing = await TestAppHarness.RunAsync(app, ["computed"]);
        var supplied = await TestAppHarness.RunAsync(app, ["computed", "custom"]);

        await Assert.That(missing.Output).IsEqualTo("computed");
        await Assert.That(supplied.Output).IsEqualTo("custom");
    }

    [Test]
    public async Task TypedParameters_SupportCustomExitCodeEnums()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands<ParametersExitCode>(commands => commands
                .AddCommand<ExitCodeCommand, ExitCodeParameters>("exit"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["exit", "ignored"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)ParametersExitCode.Failed);
    }

    [Test]
    public async Task TypedParameters_WorkWithAsyncCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<AsyncParametersCommand, AsyncParameters>("async"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["async", "hello"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello");
    }

    private sealed class CopyCommand : ICommand<CopyParameters>
    {
        public int Execute(CommandContext context, CopyParameters parameters)
        {
            context.Console.Output.Write(
                $"{parameters.Source}|{parameters.Destination ?? "<null>"}|{parameters.Label}|{string.Join(",", parameters.Extras)}");
            return 0;
        }
    }

    private sealed class ComputedCommand : ICommand<ComputedParameters>
    {
        public int Execute(CommandContext context, ComputedParameters parameters)
        {
            context.Console.Output.Write(parameters.Label);
            return 0;
        }
    }

    private sealed class ExitCodeCommand : ICommand<ExitCodeParameters, ParametersExitCode>
    {
        public ParametersExitCode Execute(CommandContext context, ExitCodeParameters parameters) =>
            ParametersExitCode.Failed;
    }

    private sealed class AsyncParametersCommand : IAsyncCommand<AsyncParameters>
    {
        public async Task<int> ExecuteAsync(CommandContext context, AsyncParameters parameters)
        {
            await Task.Yield();
            context.Console.Output.Write(parameters.Name);
            return 0;
        }
    }
}

public sealed partial class CopyParameters
{
    [Argument(0)]
    public string Source { get; set; } = "";

    [Argument(1, Optional = true)]
    public string? Destination { get; set; }

    [Argument(2, Optional = true)]
    public string Label { get; set; } = "default";

    [VariadicArgument(3)]
    public string[] Extras { get; set; } = [];
}

public sealed partial class ComputedParameters
{
    [Argument(0, Optional = true)]
    public string Label { get; set; } = CreateDefaultLabel();

    private static string CreateDefaultLabel() => "computed";
}

public sealed partial class ExitCodeParameters
{
    [Argument(0)]
    public string Name { get; set; } = "";
}

public sealed partial class AsyncParameters
{
    [Argument(0)]
    public string Name { get; set; } = "";
}

public enum ParametersExitCode
{
    Success = 0,
    Failed = 4,
}
