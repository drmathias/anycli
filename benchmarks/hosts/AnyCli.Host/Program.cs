// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Commands.Execution;
using AnyCli.Commands.Registration;
using AnyCli.Console;
using AnyCli.Parameters.Arguments;
using AnyCli.Parameters.Options;

var app = ConsoleAppBuilder.Create()
    .WithName("bench")
    .ConfigureCommands(commands => commands.AddCommand<RunCommand, RunParameters>("run"))
    .Build();

return await app.RunAsync(
        args,
        new TextConsole(Console.In, TextWriter.Null, TextWriter.Null))
    .ConfigureAwait(false);

internal sealed class RunCommand : ICommand<RunParameters>
{
    public int Execute(CommandContext context, RunParameters parameters) => 0;
}

public sealed partial class RunParameters
{
    [Argument(0)]
    public string Input { get; set; } = "";

    [Option("--count")]
    public int Count { get; set; }

    [Option("--verbose")]
    public bool Verbose { get; set; }
}
