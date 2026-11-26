// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace AnyCli.Help;

/// <summary>
/// Formats help default values using lower kebab-case for enum members.
/// </summary>
public sealed class DefaultHelpValueFormatter : IHelpValueFormatter
{
    /// <inheritdoc/>
    public string? Format(object? value, CultureInfo culture)
    {
        if (value is null) return null;
        if (value is string text) return string.IsNullOrEmpty(text) ? null : text;
        if (value is Enum enumValue) return FormatEnum(enumValue);
        if (value is IEnumerable values) return FormatItems(values, culture);
        return Convert.ToString(value, culture);
    }

    private string? FormatItems(IEnumerable values, CultureInfo culture)
    {
        var items = new List<string>();
        foreach (var item in values)
        {
            var itemText = Format(item, culture);
            if (!string.IsNullOrEmpty(itemText)) items.Add(itemText);
        }

        return items.Count == 0 ? null : string.Join(", ", items);
    }

    private static string FormatEnum(Enum value)
    {
        var policy = JsonNamingPolicy.KebabCaseLower;
        var name = value.ToString();
        return name.IndexOf(',') < 0
            ? policy.ConvertName(name)
            : string.Join(
                ", ",
                name.Split(',', StringSplitOptions.TrimEntries).Select(policy.ConvertName));
    }
}
