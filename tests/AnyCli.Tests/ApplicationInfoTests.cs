// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class ApplicationInfoTests
{
    [Test]
    public async Task Command_ExposesApplicationMetadata()
    {
        var result = await TestAppHarness.RunAsync(CreateApp(), ["ping"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            "command|anycli|ping:True:0,remote:False:1|-h,--help,-v,--version");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task Command_WithGlobalOptions_ExposesApplicationMetadata()
    {
        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("1.2.3")
            .AddGlobalOptions<TestGlobalOptions>()
            .ConfigureCommands(commands => commands.AddCommand<ApplicationGlobalCommand, TestGlobalEchoParameters>("run"))
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["run"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).StartsWith("command|anycli|run:True:0|");
        await Assert.That(result.Output).Contains("-l");
        await Assert.That(result.Output).Contains("--log-level");
        await Assert.That(result.Output).Contains("--version");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task RootHelp_ExposesApplicationMetadata()
    {
        var result = await TestAppHarness.RunAsync(CreateApp(), ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo("root|anycli|ping:True:0,remote:False:1");
        await Assert.That(result.Error).IsEmpty();
    }

    [Test]
    public async Task CommandHelp_ExposesApplicationMetadata()
    {
        var result = await TestAppHarness.RunAsync(CreateApp(), ["remote", "--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).IsEqualTo(
            "command-help|remote|anycli|ping:True:0,remote:False:1");
        await Assert.That(result.Error).IsEmpty();
    }

    private static ConsoleApp CreateApp() =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .WithVersion("1.2.3")
            .ConfigureCommands(commands =>
            {
                commands.AddCommand<ApplicationCommand>("ping");
                commands.AddGroup(
                    "remote",
                    "Manage remotes",
                    group => group.AddCommand<ApplicationCommand>("add"));
            })
            .ConfigureHelp(help => help.UseProvider<ApplicationHelpProvider>())
            .Build();

    private static string DescribeCommands(IReadOnlyList<CommandInfo> commands) =>
        string.Join(",", commands.Select(command =>
            $"{command.Name}:{command.IsExecutable}:{command.Children.Count}"));

    private sealed class ApplicationCommand : NoParametersCommand
    {
        public override int Execute(CommandContext context)
        {
            context.Console.Output.Write(
                $"command|{context.Application.AppName}|" +
                $"{DescribeCommands(context.Application.Commands)}|" +
                $"{string.Join(",", context.Application.GlobalOptions.SelectMany(option => option.Names))}");
            return 0;
        }
    }

    private sealed class ApplicationGlobalCommand : ICommand<TestGlobalEchoParameters>
    {
        public int Execute(CommandContext context, TestGlobalEchoParameters parameters)
        {
            context.Console.Output.Write(
                $"command|{context.Application.AppName}|" +
                $"{DescribeCommands(context.Application.Commands)}|" +
                $"{string.Join(",", context.Application.GlobalOptions.SelectMany(option => option.Names))}");
            return 0;
        }
    }

    private sealed class ApplicationHelpProvider : IHelpProvider
    {
        public ValueTask WriteRootHelpAsync(
            RootHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write(
                $"root|{context.Application.AppName}|" +
                $"{DescribeCommands(context.Application.Commands)}");
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteCommandHelpAsync(
            CommandHelpContext context,
            CancellationToken cancellationToken)
        {
            context.Console.Output.Write(
                $"command-help|{string.Join(' ', context.Command.Path)}|" +
                $"{context.Application.AppName}|" +
                $"{DescribeCommands(context.Application.Commands)}");
            return ValueTask.CompletedTask;
        }
    }
}
