// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Text;

namespace AnyCli.SpectreConsole;

internal sealed class OutputWriterTextWriter(IOutputWriter writer) : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value) => writer.Write(value.ToString());

    public override void Write(string? value) => writer.Write(value);

    public override void Write(char[] buffer, int index, int count) => writer.Write(new string(buffer, index, count));

    public override void Write(ReadOnlySpan<char> buffer) => writer.Write(buffer.ToString());

    public override void WriteLine() => writer.WriteLine();

    public override void WriteLine(string? value) => writer.WriteLine(value);

    public override void WriteLine(ReadOnlySpan<char> buffer) => writer.WriteLine(buffer.ToString());

    public override void Flush() { }
}
