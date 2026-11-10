// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.CommandLine;

var inputArgument = new Argument<string>("input");
var countOption = new Option<int>("--count")
{
    DefaultValueFactory = _ => 0,
};
var verboseOption = new Option<bool>("--verbose");

var runCommand = new Command("run", "Runs the benchmark command");
runCommand.Arguments.Add(inputArgument);
runCommand.Options.Add(countOption);
runCommand.Options.Add(verboseOption);
runCommand.SetAction(parseResult =>
{
    _ = parseResult.GetRequiredValue(inputArgument);
    _ = parseResult.GetValue(countOption);
    _ = parseResult.GetValue(verboseOption);
    return 0;
});

var rootCommand = new RootCommand();
rootCommand.Subcommands.Add(runCommand);

return rootCommand.Parse(args).Invoke(new InvocationConfiguration
{
    Output = TextWriter.Null,
    Error = TextWriter.Null,
});
