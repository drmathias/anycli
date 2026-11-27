// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Reflection;

using Microsoft.CodeAnalysis;

namespace AnyCli.Generators.Tests;

public class DiagnosticHelpLinkTests
{
    [Test]
    public async Task AllDiagnosticDescriptorsHaveHelpLinks()
    {
        var expectedIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "ANYCLI001",
            "ANYCLI002",
            "ANYCLI003",
            "ANYCLI004",
            "ANYCLI005",
            "ANYCLI007",
            "ANYCLI009",
            "ANYCLI013",
            "ANYCLI014",
            "ANYCLI015",
            "ANYCLI016",
            "ANYCLI017",
            "ANYCLI018",
            "ANYCLI019",
            "ANYCLI020",
            "ANYCLI022",
            "ANYCLI023",
            "ANYCLI025",
            "ANYCLI026",
        };

        var descriptors = new[]
        {
            typeof(CommandParametersGenerator),
            typeof(InterceptorUsageAnalyzer),
        }
        .SelectMany(type => type
            .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(DiagnosticDescriptor))
            .Select(field => (DiagnosticDescriptor)field.GetValue(null)!))
        .ToArray();

        await Assert.That(descriptors.Length).IsEqualTo(expectedIds.Count);

        foreach (var descriptor in descriptors)
        {
            await Assert.That(expectedIds.Contains(descriptor.Id)).IsTrue();
            await Assert.That(descriptor.HelpLinkUri).IsEqualTo(
                $"https://drmathias.github.io/anycli/reference/diagnostics/{descriptor.Id.ToLowerInvariant()}.html");
        }
    }
}
