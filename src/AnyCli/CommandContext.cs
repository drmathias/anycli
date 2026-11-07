// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

public sealed class CommandContext
{
    internal CommandContext(string name, TextWriter output, TextWriter error)
    {
        Name = name;
        Output = output;
        Error = error;
    }

    public string Name { get; }

    public TextWriter Output { get; }

    public TextWriter Error { get; }
}
