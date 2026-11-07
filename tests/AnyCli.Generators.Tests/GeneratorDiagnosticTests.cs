// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace AnyCli.Generators.Tests;

public class GeneratorDiagnosticTests
{
    private static readonly ImmutableArray<MetadataReference> References = CreateReferences();

    [Test]
    public async Task ReportsUnsupportedArgumentType()
    {
        var result = RunGenerator(
            """
            using AnyCli;

            public sealed partial class BadSettings
            {
                [Argument(0)]
                public object Value { get; set; } = new();
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI001");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Value");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsUnsupportedTypeAlongsideSupportedArguments()
    {
        var result = RunGenerator(
            """
            using AnyCli;

            public sealed partial class BadSettings
            {
                [Argument(0)]
                public string Name { get; set; } = "";

                [Argument(1)]
                public object Value { get; set; } = new();
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI001");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Value");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidArgumentParser()
    {
        var result = RunGenerator(
            """
            using AnyCli;

            public sealed class NotAParser
            {
            }

            public sealed partial class BadSettings
            {
                [Argument(0, Parser = typeof(NotAParser))]
                public int Value { get; set; }
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI002");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Value");
        await Assert.That(diagnostic.GetMessage()).Contains("NotAParser");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidVariadicArgument()
    {
        var result = RunGenerator(
            """
            using AnyCli;

            public sealed partial class BadSettings
            {
                [VariadicArgument(0)]
                public string Value { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI003");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Value");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task UsesAssemblyLevelArgumentParserRegistration()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli;

            [assembly: ArgumentParser<Widget, WidgetParser>]

            public sealed class Widget
            {
            }

            public sealed class WidgetParser : IArgumentParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }

            public sealed partial class GoodSettings
            {
                [Argument(0)]
                public Widget Widget { get; set; } = new();

                [VariadicArgument(1)]
                public Widget[] Widgets { get; set; } = [];
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(errors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsDuplicateArgumentParserRegistrations()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli;

            [assembly: ArgumentParser<Widget, WidgetParser1>]
            [assembly: ArgumentParser<Widget, WidgetParser2>]

            public sealed class Widget
            {
            }

            public sealed class WidgetParser1 : IArgumentParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }

            public sealed class WidgetParser2 : IArgumentParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI004");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Widget");
    }

    [Test]
    public async Task UsesAssemblyLevelEnumArgumentParserRegistration()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli;

            [assembly: EnumArgumentParser<ModeParser>]

            public enum Mode
            {
                Fast,
                Slow,
            }

            public sealed class ModeParser : IEnumArgumentParser
            {
                public static bool TryParse<TEnum>(
                    string value,
                    IFormatProvider? provider,
                    out TEnum result)
                    where TEnum : struct, Enum =>
                    Enum.TryParse(value, ignoreCase: false, out result);
            }

            public sealed partial class GoodSettings
            {
                [Argument(0)]
                public Mode Mode { get; set; }
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(result.GeneratedSourceTexts.Single().Contains("ParseEnumCustom")).IsTrue();
        await Assert.That(result.GeneratedSourceTexts.Single().Contains("ModeParser")).IsTrue();
        await Assert.That(errors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task GeneratesSettingsForCollectionTargets()
    {
        var result = RunGenerator(
            """
            using System.Collections.Generic;
            using AnyCli;

            public sealed partial class A
            {
                [VariadicArgument(0)]
                public IReadOnlyList<int> Values { get; set; } = [];
            }

            public sealed partial class B
            {
                [VariadicArgument(0)]
                public List<int> Values { get; set; } = [];
            }

            public sealed partial class C
            {
                [VariadicArgument(0)]
                public ICollection<int> Values { get; set; } = [];
            }

            public sealed partial class D
            {
                [VariadicArgument(0)]
                public IEnumerable<int> Values { get; set; } = [];
            }

            public sealed partial class E
            {
                [VariadicArgument(0)]
                public HashSet<int> Values { get; set; } = [];
            }

            public sealed partial class F
            {
                [VariadicArgument(0, MinimumCount = 1)]
                public int[] Values { get; set; } = [];
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(6);
        await Assert.That(errors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidVariadicMinimumCount()
    {
        var result = RunGenerator(
            """
            using AnyCli;

            public sealed partial class BadSettings
            {
                [VariadicArgument(0, MinimumCount = -1)]
                public string[] Values { get; set; } = [];
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI005");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Values");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task GeneratesSettingsForSupportedArgumentTypes()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli;

            public enum Mode
            {
                Fast,
                Slow,
            }

            public sealed class Widget
            {
            }

            public sealed class WidgetParser : IArgumentParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }

            public sealed partial class GoodSettings
            {
                [Argument(0)]
                public int Count { get; set; }

                [Argument(1)]
                public Mode Mode { get; set; }

                [Argument(2, Optional = true)]
                public string? Name { get; set; }

                [VariadicArgument(3)]
                public string[] Tags { get; set; } = [];

                [Argument(4, Parser = typeof(WidgetParser))]
                public Widget Widget { get; set; } = new();
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(errors.Length).IsEqualTo(0);
    }

    private static GeneratorResult RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "GeneratorTests",
            [syntaxTree],
            References,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new CommandSettingsGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);
        var generatedSources = driver
            .GetRunResult()
            .Results
            .SelectMany(result => result.GeneratedSources)
            .ToArray();
        var generatedSourceTexts = generatedSources
            .Select(source => source.SourceText.ToString())
            .ToImmutableArray();

        return new GeneratorResult(
            outputCompilation,
            generatorDiagnostics,
            generatedSources.Length,
            generatedSourceTexts);
    }

    private static ImmutableArray<MetadataReference> CreateReferences()
    {
        var trustedPlatformAssemblies =
            (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return
        [
            .. trustedPlatformAssemblies
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path)),
            MetadataReference.CreateFromFile(typeof(ArgumentAttribute).Assembly.Location),
        ];
    }

    private sealed record GeneratorResult(
        Compilation OutputCompilation,
        ImmutableArray<Diagnostic> GeneratorDiagnostics,
        int GeneratedSourceCount,
        ImmutableArray<string> GeneratedSourceTexts);
}
