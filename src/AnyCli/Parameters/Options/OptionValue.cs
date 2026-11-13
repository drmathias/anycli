// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Options;

internal sealed class OptionValue
{
    private string? _value;
    private List<string>? _values;

    public string? LastValue => _values is { Count: > 0 } ? _values[^1] : _value;

    public IReadOnlyList<string> Values => _values ?? (_value is null ? [] : [_value]);

    public void Add(string value, bool multiple)
    {
        if (!multiple)
        {
            _value = value;
            return;
        }

        _values ??= [];
        _values.Add(value);
    }
}
