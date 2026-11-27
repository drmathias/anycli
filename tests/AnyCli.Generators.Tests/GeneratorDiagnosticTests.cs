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

            public sealed partial class Parameters
            {
                [Argument(0)]
                public Widget Widget { get; set; } = new();
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI004");
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains("Widget");
        await Assert.That(diagnostic.GetMessage()).Contains("last registration");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).Contains("WidgetParser2");
        await Assert.That(generated).DoesNotContain("WidgetParser1");
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
                Alpha,
                Beta,
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
            using System.ComponentModel;
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
                Alpha,
                Beta,
            }

            public sealed partial class GoodParameters
            {
                [Option("--mode")]
                public Mode Mode { get; set; } = Mode.Alpha;

                [Option("--verbose")]
                public bool Verbose { get; set; } = true;

                [OptionalArgument(0)]
                public int Port { get; set; } = 8080;
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
    public async Task DoesNotCaptureComputedInitializerForHelp()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Options;

            public sealed partial class GoodParameters
            {
                [Option("--data")]
                public string Data { get; set; } = CreateDefault();

                private static string CreateDefault() => "data";
            }
            """);

        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(generated).DoesNotContain("SetDefaultValue(");
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
            using System.ComponentModel;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Options;
            using AnyCli.Parameters.Parsing;

            public sealed partial class GoodParameters
            {
                [RequiredOption("--config")]
                public string Config { get; set; } = "";

                [Option("--output", "-o")]
                [Description("Output path")]
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
                Alpha,
                Beta,
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
            using System.ComponentModel;
            using AnyCli.Parameters.Options;

            [GlobalOptions]
            public sealed partial class AppOptions
            {
                [Option("--log-level", "-l")]
                [Description("Log level")]
                public string LogLevel { get; set; } = "information";

                [Option("--verbose")]
                public bool Verbose { get; set; }

                [Option("--tag")]
                public string[] Tags { get; set; } = ["a", "b"];
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
        await Assert.That(generated).Contains(
            "BindAsync(");
        await Assert.That(generated).Contains(
            "global::System.Threading.CancellationToken cancellationToken");
        await Assert.That(generated).DoesNotContain("ICommandParameters<");
    }

    [Test]
    public async Task GeneratesGlobalOptionsForPartialRecord()
    {
        var result = RunGenerator(
            """
            using System.ComponentModel;
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
            using System.ComponentModel;
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
            using System.ComponentModel;
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
            using System.ComponentModel;
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

    [Test]
    public async Task ReportsInvalidDefaultValueProvider()
    {
        var result = RunGenerator(
            """
            using AnyCli.Parameters.Options;

            public sealed class NotAProvider
            {
            }

            public sealed partial class BadParameters
            {
                [Option("--name", DefaultValueProvider = typeof(NotAProvider))]
                public string Name { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI020");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage()).Contains("Name");
        await Assert.That(diagnostic.GetMessage()).Contains("NotAProvider");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsDefaultValueDisplayOnRequiredParameter()
    {
        var result = RunGenerator(
            """
            using AnyCli.Help;
            using AnyCli.Parameters.Options;

            public sealed partial class BadParameters
            {
                [RequiredOption("--name")]
                [DefaultValueDisplay("value")]
                public string Name { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI022");
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains("Name");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).DoesNotContain("SetDefaultValueDisplay");
    }

    [Test]
    public async Task ReportsDefaultValueDisplayWithoutDefault()
    {
        var result = RunGenerator(
            """
            using AnyCli.Help;
            using AnyCli.Parameters.Options;

            public sealed partial class BadParameters
            {
                [Option("--name")]
                [DefaultValueDisplay("value")]
                public string Name { get; set; }
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI023");
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains("Name");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).DoesNotContain("SetDefaultValueDisplay");
    }

    [Test]
    public async Task GeneratesDefaultValueProviderOnGlobalOption()
    {
        var result = RunGenerator(
            """
            using System.Threading;
            using AnyCli.Parameters.Binding;
            using AnyCli.Parameters.Options;

            public sealed class LevelProvider : IDefaultValueProvider<string>
            {
                public string CreateDefault() => "provider";
            }

            [GlobalOptions]
            public sealed partial class AppOptions
            {
                [Option("--level", DefaultValueProvider = typeof(LevelProvider))]
                public string Level { get; set; } = "";
            }
            """);

        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(generated).Contains(
            "SetDefaultValueProvider(typeof(global::LevelProvider))");
        await Assert.That(generated).Contains("LevelProvider");
        await Assert.That(generated).Contains("CreateDefault()");
    }

    [Test]
    public async Task GeneratesAsyncDefaultValueProvider()
    {
        var result = RunGenerator(
            """
            using System.Threading;
            using System.Threading.Tasks;
            using AnyCli.Parameters.Binding;
            using AnyCli.Parameters.Options;

            public sealed class NameProvider : IAsyncDefaultValueProvider<string>
            {
                public ValueTask<string> CreateDefaultAsync(CancellationToken cancellationToken) =>
                    ValueTask.FromResult("provider");
            }

            public sealed partial class ValidParameters
            {
                [Option("--name", DefaultValueProvider = typeof(NameProvider))]
                public string Name { get; set; } = "";
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(generated).Contains("BindAsync(");
        await Assert.That(generated).Contains("CreateDefaultAsync(cancellationToken)");
        await Assert.That(generated).Contains("await ");
    }

    [Test]
    public async Task GeneratesAsyncParameterParser()
    {
        var result = RunGenerator(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Parsing;

            public sealed record Widget(string Name);

            public sealed class AsyncWidgetParser : IAsyncParameterParser<Widget>
            {
                public ValueTask<ParameterParseResult<Widget>> TryParseAsync(
                    string value,
                    IFormatProvider? provider,
                    CancellationToken cancellationToken) =>
                    ValueTask.FromResult(
                        ParameterParseResult<Widget>.Succeeded(new Widget(value)));
            }

            public sealed partial class Parameters
            {
                [Argument(0, Parser = typeof(AsyncWidgetParser))]
                public Widget Widget { get; set; } = new("");
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(generated).Contains("async global::System.Threading.Tasks.ValueTask");
        await Assert.That(generated).Contains("ParseCustomAsync");
        await Assert.That(generated).Contains("cancellationToken");
    }

    [Test]
    public async Task GeneratesAsyncEnumParameterParser()
    {
        var result = RunGenerator(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Parsing;

            [assembly: EnumParameterParser<AsyncModeParser>]

            public enum Mode
            {
                Alpha,
                Beta,
            }

            public sealed class AsyncModeParser : IAsyncEnumParameterParser
            {
                public ValueTask<ParameterParseResult<TEnum>> TryParseAsync<TEnum>(
                    string value,
                    IFormatProvider? provider,
                    CancellationToken cancellationToken)
                    where TEnum : struct, Enum =>
                    ValueTask.FromResult(
                        Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
                            ? ParameterParseResult<TEnum>.Succeeded(result)
                            : ParameterParseResult<TEnum>.Failed);
            }

            public sealed partial class Parameters
            {
                [Argument(0)]
                public Mode Mode { get; set; }
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(generated).Contains("async global::System.Threading.Tasks.ValueTask");
        await Assert.That(generated).Contains("ParseEnumCustomAsync");
    }

    [Test]
    public async Task ReportsAmbiguousParameterParser()
    {
        var result = RunGenerator(
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using AnyCli.Parameters.Arguments;
            using AnyCli.Parameters.Parsing;

            public sealed record Widget(string Name);

            public sealed class AmbiguousParser :
                IParameterParser<Widget>,
                IAsyncParameterParser<Widget>
            {
                public static bool TryParse(
                    string value,
                    IFormatProvider? provider,
                    out Widget result)
                {
                    result = new Widget(value);
                    return true;
                }

                public ValueTask<ParameterParseResult<Widget>> TryParseAsync(
                    string value,
                    IFormatProvider? provider,
                    CancellationToken cancellationToken) =>
                    ValueTask.FromResult(
                        ParameterParseResult<Widget>.Succeeded(new Widget(value)));
            }

            public sealed partial class BadParameters
            {
                [Argument(0, Parser = typeof(AmbiguousParser))]
                public Widget Widget { get; set; } = new("");
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI026");
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains("Widget");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).Contains("ParseCustomAsync");
    }

    [Test]
    public async Task ReportsAmbiguousDefaultValueProvider()
    {
        var result = RunGenerator(
            """
            using System.Threading;
            using System.Threading.Tasks;
            using AnyCli.Parameters.Binding;
            using AnyCli.Parameters.Options;

            public sealed class NameProvider :
                IDefaultValueProvider<string>,
                IAsyncDefaultValueProvider<string>
            {
                public string CreateDefault() => "sync";

                public ValueTask<string> CreateDefaultAsync(CancellationToken cancellationToken) =>
                    ValueTask.FromResult("async");
            }

            public sealed partial class BadParameters
            {
                [Option("--name", DefaultValueProvider = typeof(NameProvider))]
                public string Name { get; set; } = "";
            }
            """);

        var diagnostic = result.GeneratorDiagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI025");
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Warning);
        await Assert.That(diagnostic.GetMessage()).Contains("Name");
        await Assert.That(result.GeneratedSourceCount).IsEqualTo(1);
        await Assert.That(generated).Contains("CreateDefaultAsync(cancellationToken)");
    }

    [Test]
    public async Task GeneratesDefaultValueProviderAndDisplay()
    {
        var result = RunGenerator(
            """
            using System.Threading;
            using AnyCli.Help;
            using AnyCli.Parameters.Binding;
            using AnyCli.Parameters.Options;

            public sealed class NameProvider : IDefaultValueProvider<string>
            {
                public string CreateDefault() => "provider";
            }

            public sealed partial class ValidParameters
            {
                [Option("--name", DefaultValueProvider = typeof(NameProvider))]
                [DefaultValueDisplay("provider")]
                [DefaultValueDisplayResource("NameDefault")]
                public string Name { get; set; } = "";
            }
            """);

        var errors = result.OutputCompilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        var generated = result.GeneratedSourceTexts.Single();

        await Assert.That(result.GeneratorDiagnostics.Length).IsEqualTo(0);
        await Assert.That(errors.Length).IsEqualTo(0);
        await Assert.That(generated).Contains("SetDefaultValueProvider(typeof(global::NameProvider))");
        await Assert.That(generated).Contains(
            "new global::AnyCli.Help.HelpText(\"provider\", \"NameDefault\")");
        await Assert.That(generated).Contains(
            "DefaultValueProviderResolver.Resolve<global::NameProvider>(services)");
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
