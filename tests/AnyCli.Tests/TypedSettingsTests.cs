using AnyCli;

namespace AnyCli.Tests;

public class TypedSettingsTests
{
    [Test]
    public async Task TypedSettings_BindRequiredAndOptionalArguments()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<CopyCommand, CopySettings>("copy", "Copies files"))
            .Build();

        var result = await Run(app, ["copy", "source.txt"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|<null>|default|");
    }

    [Test]
    public async Task TypedSettings_BindVariadicArguments()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<CopyCommand, CopySettings>("copy", "Copies files"))
            .Build();

        var result = await Run(app, ["copy", "source.txt", "dest.txt", "fast", "a", "b"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("source.txt|dest.txt|fast|a,b");
    }

    [Test]
    public async Task TypedSettings_UseComputedPropertyInitializerAsDefault()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<ComputedCommand, ComputedSettings>("computed"))
            .Build();

        var missing = await Run(app, ["computed"]);
        var supplied = await Run(app, ["computed", "custom"]);

        await Assert.That(missing.Output).IsEqualTo("computed");
        await Assert.That(supplied.Output).IsEqualTo("custom");
    }

    [Test]
    public async Task TypedSettings_SupportCustomExitCodeEnums()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands<SettingsExitCode>(commands => commands
                .AddCommand<ExitCodeCommand, ExitCodeSettings>("exit"))
            .Build();

        var result = await Run(app, ["exit", "ignored"]);

        await Assert.That(result.ExitCode).IsEqualTo((int)SettingsExitCode.Failed);
    }

    [Test]
    public async Task TypedSettings_WorkWithAsyncCommands()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<AsyncSettingsCommand, AsyncSettings>("async"))
            .Build();

        var result = await Run(app, ["async", "hello"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("hello");
    }

    private static async Task<RunResult> Run(ConsoleApp app, string[] args)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = await app.RunAsync(
            args,
            new TextConsole(new StringReader(string.Empty), output, error));

        return new RunResult(exitCode, output.ToString(), error.ToString());
    }

    private sealed record RunResult(int ExitCode, string Output, string Error);

    private sealed class CopyCommand : ICommand<CopySettings>
    {
        public int Execute(CommandContext context, CopySettings settings)
        {
            context.Console.Output.Write(
                $"{settings.Source}|{settings.Destination ?? "<null>"}|{settings.Label}|{string.Join(",", settings.Extras)}");
            return 0;
        }
    }

    private sealed class ComputedCommand : ICommand<ComputedSettings>
    {
        public int Execute(CommandContext context, ComputedSettings settings)
        {
            context.Console.Output.Write(settings.Label);
            return 0;
        }
    }

    private sealed class ExitCodeCommand : ICommand<ExitCodeSettings, SettingsExitCode>
    {
        public SettingsExitCode Execute(CommandContext context, ExitCodeSettings settings) =>
            SettingsExitCode.Failed;
    }

    private sealed class AsyncSettingsCommand : IAsyncCommand<AsyncSettings>
    {
        public async Task<int> ExecuteAsync(CommandContext context, AsyncSettings settings)
        {
            await Task.Yield();
            context.Console.Output.Write(settings.Name);
            return 0;
        }
    }
}

public sealed partial class CopySettings
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

public sealed partial class ComputedSettings
{
    [Argument(0, Optional = true)]
    public string Label { get; set; } = CreateDefaultLabel();

    private static string CreateDefaultLabel() => "computed";
}

public sealed partial class ExitCodeSettings
{
    [Argument(0)]
    public string Name { get; set; } = "";
}

public sealed partial class AsyncSettings
{
    [Argument(0)]
    public string Name { get; set; } = "";
}

public enum SettingsExitCode
{
    Success = 0,
    Failed = 4,
}
