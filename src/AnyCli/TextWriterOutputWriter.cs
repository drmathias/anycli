// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class TextWriterOutputWriter(TextWriter writer) : IOutputWriter
{
    public void Write(string? value) => writer.Write(value);

    public void WriteLine() => writer.WriteLine();

    public void WriteLine(string? value) => writer.WriteLine(value);
}
