// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

var app = new CommandLineApplicationBuilder()
    .AddCommand(RunCommand.Descriptor)
    .UseConsole(new FakeConsole())
    .Build();

return await app.RunAsync(args, new Dictionary<string, string>()).ConfigureAwait(false);

[Command("run")]
internal sealed partial class RunCommand : ICommand
{
    [CommandParameter(0)]
    public required string Input { get; set; }

    [CommandOption("count")]
    public int Count { get; set; }

    [CommandOption("verbose")]
    public bool Verbose { get; set; }

    public ValueTask ExecuteAsync(IConsole console) => default;
}
