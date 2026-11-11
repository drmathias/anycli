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
    public async Task ReportsUnsupportedParameterType()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
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
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
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
    public async Task ReportsInvalidParameterParser()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed class NotAParser
            {
            }

            public sealed partial class BadParameters
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
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
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
    public async Task UsesAssemblyLevelParameterParserRegistration()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            [assembly: ParameterValueParser<Widget, WidgetParser>]

            public sealed class Widget
            {
            }

            public sealed class WidgetParser : IParameterParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }

            public sealed partial class GoodParameters
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
    public async Task ReportsDuplicateParameterParserRegistrations()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            [assembly: ParameterValueParser<Widget, WidgetParser1>]
            [assembly: ParameterValueParser<Widget, WidgetParser2>]

            public sealed class Widget
            {
            }

            public sealed class WidgetParser1 : IParameterParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }

            public sealed class WidgetParser2 : IParameterParser<Widget>
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
    public async Task UsesAssemblyLevelEnumParameterParserRegistration()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            [assembly: EnumParameterParser<ModeParser>]

            public enum Mode
            {
                Fast,
                Slow,
            }

            public sealed class ModeParser : IEnumParameterParser
            {
                public static bool TryParse<TEnum>(
                    string value,
                    IFormatProvider? provider,
                    out TEnum result)
                    where TEnum : struct, Enum =>
                    Enum.TryParse(value, ignoreCase: false, out result);
            }

            public sealed partial class GoodParameters
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
    public async Task GeneratesParametersForCollectionTargets()
    {
        var result = RunGenerator(
            """
            using System.Collections.Generic;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

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
    public async Task ReportsIncompatibleDefaultValueType()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--count", DefaultValue = "not-an-int")]
                public int Count { get; set; }
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI006");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Count");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidCollectionDefaultValue()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--tag", DefaultValue = 5)]
                public string[] Tags { get; set; } = [];
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI010");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Tags");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidCollectionDefaultValueElementType()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--count", DefaultValue = new[] { "not-an-int" })]
                public int[] Counts { get; set; } = [];
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI011");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Counts");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsDefaultValueOnRequiredVariadicParameter()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [VariadicArgument(0, MinimumCount = 1, DefaultValue = new[] { "a" })]
                public string[] Values { get; set; } = [];
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI012");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Values");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsNegativeArgumentPosition()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Argument(-1)]
                public string Value { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI007");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Value");
        await Assert.That(diagnostic.GetMessage()).Contains("-1");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsDuplicateArgumentPositions()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Argument(0)]
                public string First { get; set; } = "";

                [Argument(0)]
                public string Second { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI007");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Second");
        await Assert.That(diagnostic.GetMessage()).Contains("0");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsNonContiguousArgumentPositions()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Argument(0)]
                public string First { get; set; } = "";

                [Argument(2)]
                public string Third { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI007");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Third");
        await Assert.That(diagnostic.GetMessage()).Contains("2");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidOptionName()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--bad name")]
                public string Output { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI009");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Output");
        await Assert.That(diagnostic.GetMessage()).Contains("--bad name");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidShortOptionName()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("-output")]
                public string Output { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI009");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Output");
        await Assert.That(diagnostic.GetMessage()).Contains("-output");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidOptionAlias()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--output", "-o", "bad alias")]
                public string Output { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI013");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Output");
        await Assert.That(diagnostic.GetMessage()).Contains("bad alias");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsDuplicateOptionNames()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--output", "-o", "-o")]
                public string Output { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI014");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Output");
        await Assert.That(diagnostic.GetMessage()).Contains("-o");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsDuplicateOptionNamesAcrossProperties()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [Option("--output")]
                public string Output { get; set; } = "";

                [Option("-o", "--output")]
                public string SecondOutput { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI014");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("SecondOutput");
        await Assert.That(diagnostic.GetMessage()).Contains("--output");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task GeneratesTypedDefaults()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public enum Mode
            {
                Fast,
                Slow,
            }

            public sealed partial class GoodParameters
            {
                [Option("--mode", DefaultValue = Mode.Fast)]
                public Mode Mode { get; set; }

                [Option("--verbose", DefaultValue = true)]
                public bool Verbose { get; set; }

                [OptionalArgument(0, DefaultValue = 8080)]
                public int Port { get; set; }
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(result.GeneratedSourceTexts.Single().Contains("SetDefaultValue")).IsTrue();
        await Assert.That(errors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidVariadicMinimumCount()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
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
    public async Task ReportsInvalidVariadicMaximumCountBelowMinimum()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [VariadicArgument(0, MinimumCount = 3, MaximumCount = 2)]
                public string[] Values { get; set; } = [];
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI017");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Values");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsInvalidVariadicMaximumCountNotVariadic()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class BadParameters
            {
                [VariadicArgument(0, MaximumCount = 1)]
                public string[] Values { get; set; } = [];
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI017");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Values");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task GeneratesParametersForOptions()
    {
        var result = RunGenerator(
            """
            using System.Collections.Generic;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class GoodParameters
            {
                [RequiredOption("--config")]
                public string Config { get; set; } = "";

                [Option("--output", "-o", Description = "Output path")]
                public string Output { get; set; } = "";

                [Option("--verbose", "-v")]
                public bool Verbose { get; set; }

                [Option("--tag")]
                public List<int> Tags { get; set; } = [];

                [Option]
                public bool DryRun { get; set; }
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
    public async Task GeneratesParametersForSupportedParameterTypes()
    {
        var result = RunGenerator(
            """
            using System;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public enum Mode
            {
                Fast,
                Slow,
            }

            public sealed class Widget
            {
            }

            public sealed class WidgetParser : IParameterParser<Widget>
            {
                public static bool TryParse(string value, IFormatProvider? provider, out Widget result)
                {
                    result = new Widget();
                    return true;
                }
            }

            public sealed partial class GoodParameters
            {
                [Argument(0)]
                public int Count { get; set; }

                [Argument(1)]
                public Mode Mode { get; set; }

                [OptionalArgument(2)]
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

    [Test]
    public async Task GeneratesGlobalOptions()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Options;

            [GlobalOptions]
            public sealed partial class AppOptions
            {
                [Option("--log-level", "-l", Description = "Log level")]
                public string LogLevel { get; set; } = "information";

                [Option("--verbose")]
                public bool Verbose { get; set; }

                [Option("--tag", DefaultValue = new[] { "a", "b" })]
                public string[] Tags { get; set; } = [];
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).Contains("IGlobalOptions<");
        await Assert.That(generated).Contains("GlobalOptionBuilder options");
        await Assert.That(generated).Contains("Bind(global::AnyCli.Parameters.Options.CommandOptionValues values)");
        await Assert.That(generated).DoesNotContain("ICommandParameters<");
    }

    [Test]
    public async Task GeneratesGlobalOptionsForPartialRecord()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Options;

            [GlobalOptions]
            public sealed partial record AppOptions
            {
                [Option]
                public int Value { get; set; }
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).Contains("partial record AppOptions");
    }

    [Test]
    public async Task GeneratesParametersWithInheritedGlobalOptions()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;

            [GlobalOptions]
            public partial class AppOptions
            {
                [Option("--log-level")]
                public string LogLevel { get; set; } = "information";
            }

            public sealed partial class RunParameters : AppOptions
            {
                [Argument(0)]
                public string Name { get; set; } = "";
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts
            .Single(source => source.Contains("partial class RunParameters"));

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(2);
        await Assert.That(generated).Contains("global::AnyCli.Parameters.Options.CommandOptionValues globalOptionValues");
        await Assert.That(generated).Contains("globalOptionValues.TryGetValue(\"--log-level\"");
        await Assert.That(generated).DoesNotContain("Options.AddValue<string>(\"--log-level\"");
    }

    [Test]
    public async Task GeneratesGlobalOptionsWithoutProperties()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Options;

            [GlobalOptions]
            public sealed partial class EmptyOptions
            {
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
    }

    [Test]
    public async Task ReportsPositionalArgumentOnGlobalOptions()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;

            [GlobalOptions]
            public sealed partial class BadOptions
            {
                [Argument(0)]
                public string Value { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI015");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Value");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
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
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new CommandParametersGenerator());
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
