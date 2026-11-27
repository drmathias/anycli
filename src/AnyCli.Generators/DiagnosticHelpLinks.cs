// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Generators;

internal static class DiagnosticHelpLinks
{
    private const string BaseUrl = "https://drmathias.github.io/anycli/reference/diagnostics/";

    public static string For(string id) => BaseUrl + id.ToLowerInvariant() + ".html";
}
