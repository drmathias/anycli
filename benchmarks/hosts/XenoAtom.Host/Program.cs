// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using XenoAtom.CommandLine;

var app = new CommandApp("bench")
{
    new Command("run")
    {
        { "<input>", "Input file", _ => { } },
        { "count=", "Count", (int _) => { } },
        { "verbose", "Verbose", _ => { } },
        (_, _) => ValueTask.FromResult(0),
    },
};

return await app.RunAsync(
        args,
        new CommandRunConfig
        {
            Out = TextWriter.Null,
            Error = TextWriter.Null,
        })
    .ConfigureAwait(false);
