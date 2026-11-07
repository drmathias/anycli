// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal interface IEnumParser
{
    bool TryParse<TEnum>(string value, IFormatProvider? provider, out TEnum result)
        where TEnum : struct, Enum;
}

internal sealed class EnumParserAdapter<TParser> : IEnumParser
    where TParser : IEnumArgumentParser
{
    public static EnumParserAdapter<TParser> Instance { get; } = new();

    public bool TryParse<TEnum>(string value, IFormatProvider? provider, out TEnum result)
        where TEnum : struct, Enum =>
        TParser.TryParse(value, provider, out result);
}
