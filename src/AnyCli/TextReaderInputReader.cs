// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class TextReaderInputReader(TextReader reader) : IInputReader
{
    public string? ReadLine() => reader.ReadLine();
}
