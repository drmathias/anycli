using AnyCli;

namespace AnyCli.Tests;

public class PositionalArgumentTests
{
    [Test]
    public async Task RequiredArgument_IsBound()
    {
        var app = CreateApp<EchoRequiredCommand>(
            "echo",
            arguments => arguments.Add("name"));

        var result = await Run(app, ["echo", "adam"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("adam");
    }

    [Test]
    public async Task MissingRequiredArgument_Throws()
    {
        var app = CreateApp<EchoRequiredCommand>(
            "echo",
            arguments => arguments.Add("name"));

        var thrown = false;
        try
        {
            await Run(app, ["echo"]);
        }
        catch (CommandArgumentException exception)
        {
            thrown = true;
            await Assert.That(exception.Message).Contains("name");
        }

        await Assert.That(thrown).IsTrue();
    }

    [Test]
    public async Task OptionalArgument_DefaultsToNull()
    {
        var app = CreateApp<EchoOptionalCommand>(
            "echo",
            arguments => arguments.AddOptional("name"));

        var missing = await Run(app, ["echo"]);
        var supplied = await Run(app, ["echo", "adam"]);

        await Assert.That(missing.Output).IsEqualTo("<null>");
        await Assert.That(supplied.Output).IsEqualTo("adam");
    }

    [Test]
    public async Task OptionalArgument_UsesConstantDefault()
    {
        var app = CreateApp<EchoOptionalCommand>(
            "echo",
            arguments => arguments.AddOptional("name", "world"));

        var result = await Run(app, ["echo"]);

        await Assert.That(result.Output).IsEqualTo("world");
    }

    [Test]
    public async Task OptionalArgument_UsesComputedDefaultOnlyWhenMissing()
    {
        var calls = 0;
        var app = CreateApp<EchoOptionalCommand>(
            "echo",
            arguments => arguments.AddOptional("name", () =>
            {
                calls++;
                return "computed";
            }));

        var supplied = await Run(app, ["echo", "adam"]);
        var missing = await Run(app, ["echo"]);

        await Assert.That(supplied.Output).IsEqualTo("adam");
        await Assert.That(missing.Output).IsEqualTo("computed");
        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task VariadicArgument_ConsumesRemainingValues()
    {
        var app = CreateApp<EchoVariadicCommand>(
            "echo",
            arguments => arguments.AddVariadic("items"));

        var many = await Run(app, ["echo", "a", "b", "c"]);
        var none = await Run(app, ["echo"]);

        await Assert.That(many.Output).IsEqualTo("a,b,c");
        await Assert.That(none.Output).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task RequiredOptionalAndVariadic_BindInOrder()
    {
        var app = CreateApp<EchoCombinedCommand>(
            "echo",
            arguments => arguments
                .Add("first")
                .AddOptional("second")
                .AddVariadic("rest"));

        var result = await Run(app, ["echo", "a", "b", "c", "d"]);

        await Assert.That(result.Output).IsEqualTo("a|b|c,d");
    }

    [Test]
    public async Task AsyncCommand_ReceivesArguments()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddAsyncCommand<AsyncEchoCommand>(
                "echo",
                "desc",
                arguments => arguments.Add("name")))
            .Build();

        var result = await Run(app, ["echo", "adam"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("adam");
    }

    private static ConsoleApp CreateApp<TCommand>(
        string name,
        Action<CommandArgumentBuilder> configureArguments)
        where TCommand : class, ICommand, new() =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand>(name, "desc", configureArguments))
            .Build();

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

    private sealed class EchoRequiredCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Console.Output.Write(context.Arguments.GetRequired("name"));
            return 0;
        }
    }

    private sealed class EchoOptionalCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Console.Output.Write(context.Arguments.GetOptional("name") ?? "<null>");
            return 0;
        }
    }

    private sealed class EchoVariadicCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            context.Console.Output.Write(string.Join(",", context.Arguments.GetVariadic("items")));
            return 0;
        }
    }

    private sealed class EchoCombinedCommand : ICommand
    {
        public int Execute(CommandContext context)
        {
            var first = context.Arguments.GetRequired("first");
            var second = context.Arguments.GetOptional("second") ?? "<null>";
            var rest = string.Join(",", context.Arguments.GetVariadic("rest"));
            context.Console.Output.Write($"{first}|{second}|{rest}");
            return 0;
        }
    }

    private sealed class AsyncEchoCommand : IAsyncCommand
    {
        public async Task<int> ExecuteAsync(CommandContext context)
        {
            await Task.Yield();
            context.Console.Output.Write(context.Arguments.GetRequired("name"));
            return 0;
        }
    }
}
