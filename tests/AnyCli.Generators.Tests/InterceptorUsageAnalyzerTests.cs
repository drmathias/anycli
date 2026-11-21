// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Globalization;

using AnyCli.Interceptors;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnyCli.Generators.Tests;

public class InterceptorUsageAnalyzerTests
{
    private static readonly ImmutableArray<MetadataReference> References = CreateReferences();

    [Test]
    public async Task AcceptsMatchingGlobalOptionsAndExitCode()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddGlobalOptions<TestOptions>()
                .ConfigureCommands<TestExitCode>(_ => { })
                .AddInterceptor<CombinedInterceptor>()
                .Build();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AcceptsMatchingGlobalOptionsWithoutExitCode()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddGlobalOptions<TestOptions>()
                .AddInterceptor<GlobalInterceptor>()
                .Build();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AcceptsMatchingExitCodeWithoutGlobalOptions()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .ConfigureCommands<TestExitCode>(_ => { })
                .AddInterceptor<ExitInterceptor>()
                .Build();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AcceptsOptionsConfiguredAfterInterceptor()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddInterceptor<CombinedInterceptor>()
                .AddGlobalOptions<TestOptions>()
                .ConfigureCommands<TestExitCode>(_ => { })
                .Build();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ReportsMissingGlobalOptions()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddInterceptor<GlobalInterceptor>()
                .Build();
            """);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI018");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("TestOptions");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("none");
    }

    [Test]
    public async Task ReportsMismatchedGlobalOptions()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddGlobalOptions<OtherOptions>()
                .AddInterceptor<GlobalInterceptor>()
                .Build();
            """);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI018");

        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("TestOptions");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("OtherOptions");
    }

    [Test]
    public async Task ReportsMissingExitCode()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddInterceptor<ExitInterceptor>()
                .Build();
            """);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI019");

        await Assert.That(diagnostic.Severity).IsEqualTo(DiagnosticSeverity.Error);
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("TestExitCode");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("int");
    }

    [Test]
    public async Task ReportsMismatchedExitCode()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .ConfigureCommands<OtherExitCode>(_ => { })
                .AddInterceptor<ExitInterceptor>()
                .Build();
            """);

        var diagnostic = diagnostics.Single(diagnostic => diagnostic.Id == "ANYCLI019");

        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("TestExitCode");
        await Assert.That(diagnostic.GetMessage(CultureInfo.InvariantCulture)).Contains("OtherExitCode");
    }

    [Test]
    public async Task ReportsBothMissingRequirementsForCombinedInterceptor()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddInterceptor<CombinedInterceptor>()
                .Build();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(2);
        await Assert.That(diagnostics.Any(diagnostic => diagnostic.Id == "ANYCLI018")).IsTrue();
        await Assert.That(diagnostics.Any(diagnostic => diagnostic.Id == "ANYCLI019")).IsTrue();
    }

    [Test]
    public async Task ReportsMissingGlobalOptionsFromLocalBuilder()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            var builder = ConsoleAppBuilder.Create();
            builder.AddInterceptor<GlobalInterceptor>();
            """);

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.Id == "ANYCLI018")).IsTrue();
    }

    [Test]
    public async Task ReportsOnlyMissingExitCodeForCombinedInterceptor()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            ConsoleAppBuilder.Create()
                .AddGlobalOptions<TestOptions>()
                .AddInterceptor<CombinedInterceptor>()
                .Build();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(1);
        await Assert.That(diagnostics[0].Id).IsEqualTo("ANYCLI019");
    }

    [Test]
    public async Task AcceptsConfigurationFromLocalBuilderInitializer()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            var builder = ConsoleAppBuilder.Create()
                .AddGlobalOptions<TestOptions>()
                .ConfigureCommands<TestExitCode>(_ => { });
            builder.AddInterceptor<CombinedInterceptor>();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AcceptsConfigurationFromSeparateCallsOnLocalBuilder()
    {
        var diagnostics = await RunAnalyzerAsync(
            """
            var builder = ConsoleAppBuilder.Create();
            builder.AddGlobalOptions<TestOptions>();
            builder.ConfigureCommands<TestExitCode>(_ => { });
            builder.AddInterceptor<CombinedInterceptor>();
            """);

        await Assert.That(diagnostics.Length).IsEqualTo(0);
    }

    private static async Task<ImmutableArray<Diagnostic>> RunAnalyzerAsync(string body)
    {
        var source = $$"""
            using System;
            using System.Threading;
            using System.Threading.Tasks;
            using AnyCli.Commands.Execution;
            using AnyCli.Commands.Registration;
            using AnyCli.Interceptors;
            using AnyCli.Parameters.Options;

            public sealed class TestOptions : IGlobalOptions<TestOptions>
            {
                public static void Configure(GlobalOptionBuilder options)
                {
                }

                public static TestOptions Bind(CommandOptionValues values) => new();
            }

            public sealed class OtherOptions : IGlobalOptions<OtherOptions>
            {
                public static void Configure(GlobalOptionBuilder options)
                {
                }

                public static OtherOptions Bind(CommandOptionValues values) => new();
            }

            public enum TestExitCode
            {
                Success = 0,
            }

            public enum OtherExitCode
            {
                Success = 0,
            }

            public sealed class GlobalInterceptor : ICommandInterceptor<TestOptions>
            {
                public Task<int> InterceptAsync(
                    CommandInterceptorContext<TestOptions> context,
                    Func<CancellationToken, Task<int>> next,
                    CancellationToken cancellationToken) => next(cancellationToken);
            }

            public sealed class ExitInterceptor : ICommandInterceptorWithExitCode<TestExitCode>
            {
                public Task<TestExitCode> InterceptAsync(
                    CommandInterceptorContext context,
                    Func<CancellationToken, Task<TestExitCode>> next,
                    CancellationToken cancellationToken) => next(cancellationToken);
            }

            public sealed class CombinedInterceptor
                : ICommandInterceptorWithExitCode<TestOptions, TestExitCode>
            {
                public Task<TestExitCode> InterceptAsync(
                    CommandInterceptorContext<TestOptions> context,
                    Func<CancellationToken, Task<TestExitCode>> next,
                    CancellationToken cancellationToken) => next(cancellationToken);
            }

            public static class App
            {
                public static void Run()
                {
            {{body}}
                }
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "InterceptorAnalyzerTests",
            [syntaxTree],
            References,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation
            .GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        if (errors.Length > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(error => error.ToString())));
        }

        var analyzer = new InterceptorUsageAnalyzer();
        var withAnalyzers = compilation.WithAnalyzers([analyzer]);

        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static ImmutableArray<MetadataReference> CreateReferences()
    {
        var trustedPlatformAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return
        [
            .. trustedPlatformAssemblies
                .Split(Path.PathSeparator)
                .Select(path => MetadataReference.CreateFromFile(path)),
            MetadataReference.CreateFromFile(typeof(ICommandInterceptor).Assembly.Location),
        ];
    }
}
