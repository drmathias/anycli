// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace AnyCli.Generators;

/// <summary>
/// Validates that interceptors are registered on a builder with the global options and exit code
/// types they require.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InterceptorUsageAnalyzer : DiagnosticAnalyzer
{
    private const string BuilderNamespace = "AnyCli.Commands.Registration";
    private const string InterceptorsNamespace = "AnyCli.Interceptors";

    private static readonly DiagnosticDescriptor MissingGlobalOptionsDiagnostic = new(
        id: "ANYCLI018",
        title: "Interceptor global options mismatch",
        messageFormat:
            "Interceptor '{0}' requires global options of type '{1}', but the application configures '{2}'; call AddGlobalOptions<{1}>() on the application builder",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingExitCodeDiagnostic = new(
        id: "ANYCLI019",
        title: "Interceptor exit code mismatch",
        messageFormat:
            "Interceptor '{0}' requires exit code type '{1}', but the application configures '{2}'; call ConfigureCommands<{1}>(...) on the application builder",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MissingGlobalOptionsDiagnostic, MissingExitCodeDiagnostic];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (!IsAddInterceptor(invocation) ||
            invocation.TargetMethod.TypeArguments.Length != 1 ||
            invocation.TargetMethod.TypeArguments[0] is not INamedTypeSymbol interceptorType) return;

        var (globalOptions, exitCode) = GetRequirements(interceptorType);
        if (globalOptions is null && exitCode is null) return;

        var configuration = GetConfiguration(invocation);
        if (!configuration.IsKnown) return;

        var interceptorName = interceptorType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        if (globalOptions is not null && !SymbolEqualityComparer.Default.Equals(globalOptions, configuration.GlobalOptions))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MissingGlobalOptionsDiagnostic,
                invocation.Syntax.GetLocation(),
                interceptorName,
                globalOptions.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat),
                configuration.GlobalOptions?.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "none"));
        }

        if (exitCode is not null && !SymbolEqualityComparer.Default.Equals(exitCode, configuration.ExitCode))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MissingExitCodeDiagnostic,
                invocation.Syntax.GetLocation(),
                interceptorName,
                exitCode.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat),
                configuration.ExitCode?.ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat) ?? "int"));
        }
    }

    private static bool IsAddInterceptor(IInvocationOperation invocation) =>
        invocation.TargetMethod.Name == "AddInterceptor" &&
        invocation.TargetMethod.IsGenericMethod &&
        IsBuilderType(invocation.TargetMethod.ContainingType);

    private static bool IsBuilderType(INamedTypeSymbol type) =>
        type.ContainingNamespace.ToDisplayString() == BuilderNamespace &&
        (type.Name == "ConsoleAppBuilder" ||
         type.Name == "ConfiguredConsoleAppBuilder");

    private static (INamedTypeSymbol? GlobalOptions, INamedTypeSymbol? ExitCode) GetRequirements(INamedTypeSymbol interceptorType)
    {
        INamedTypeSymbol? globalOptions = null;
        INamedTypeSymbol? exitCode = null;

        foreach (var @interface in interceptorType.AllInterfaces)
        {
            if (@interface.ContainingNamespace.ToDisplayString() != InterceptorsNamespace) continue;

            if (@interface.Name == "ICommandInterceptor" &&
                @interface.Arity == 1 &&
                @interface.TypeArguments[0] is INamedTypeSymbol typeofGlobalOptions) globalOptions = typeofGlobalOptions;
            else if (@interface.Name == "ICommandInterceptorWithExitCode")
            {
                if (@interface.Arity == 1 &&
                    @interface.TypeArguments[0] is INamedTypeSymbol typeofExitCode) exitCode = typeofExitCode;
                else if (@interface.Arity == 2 &&
                         @interface.TypeArguments[0] is INamedTypeSymbol combinedGlobalOptions &&
                         @interface.TypeArguments[1] is INamedTypeSymbol combinedExitCode)
                {
                    globalOptions = combinedGlobalOptions;
                    exitCode = combinedExitCode;
                }
            }
        }

        return (globalOptions, exitCode);
    }

    private static BuilderConfiguration GetConfiguration(IInvocationOperation invocation)
    {
        var configuration = new BuilderConfiguration();
        var receiver = invocation.Instance;

        var chainRoot = invocation;
        while (chainRoot.Parent is IInvocationOperation parentInvocation) chainRoot = parentInvocation;

        CollectConfiguration(chainRoot, configuration);

        if (receiver is ILocalReferenceOperation localReference)
        {
            var local = localReference.Local;
            IOperation root = invocation;
            while (root.Parent is not null) root = root.Parent;

            foreach (var candidate in EnumerateInvocations(root))
            {
                if (UsesLocal(candidate.Instance, local)) AddConfiguration(candidate, configuration);
            }

            foreach (var syntaxReference in local.DeclaringSyntaxReferences)
            {
                if (syntaxReference.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: { } initializer }) continue;

                var semanticModel = invocation.SemanticModel;
                if (semanticModel is null)
                {
                    configuration.IsKnown = false;
                    return configuration;
                }

                var initializerOperation = semanticModel.GetOperation(initializer, CancellationToken.None);

                CollectConfiguration(initializerOperation, configuration);
            }
        }
        else if (receiver is not IInvocationOperation) configuration.IsKnown = false;

        return configuration;
    }

    private static void CollectConfiguration(
        IOperation? operation,
        BuilderConfiguration configuration)
    {
        if (operation is null) return;

        if (operation is IInvocationOperation invocation) AddConfiguration(invocation, configuration);
        foreach (var child in operation.ChildOperations) CollectConfiguration(child, configuration);
    }

    private static void AddConfiguration(IInvocationOperation invocation, BuilderConfiguration configuration)
    {
        if (!IsBuilderType(invocation.TargetMethod.ContainingType) ||
            !invocation.TargetMethod.IsGenericMethod ||
            invocation.TargetMethod.TypeArguments.Length != 1 ||
            invocation.TargetMethod.TypeArguments[0] is not INamedTypeSymbol configuredType) return;

        if (invocation.TargetMethod.Name == "AddGlobalOptions") configuration.GlobalOptions = configuredType;
        else if (invocation.TargetMethod.Name == "ConfigureCommands") configuration.ExitCode = configuredType;
    }

    private static IEnumerable<IInvocationOperation> EnumerateInvocations(IOperation operation)
    {
        if (operation is IInvocationOperation invocation) yield return invocation;

        foreach (var child in operation.ChildOperations)
        {
            foreach (var descendant in EnumerateInvocations(child)) yield return descendant;
        }
    }

    private static bool UsesLocal(IOperation? operation, ILocalSymbol local)
    {
        if (operation is null) return false;
        if (operation is ILocalReferenceOperation localReference && SymbolEqualityComparer.Default.Equals(localReference.Local, local)) return true;
        foreach (var child in operation.ChildOperations) if (UsesLocal(child, local)) return true;
        return false;
    }

    private sealed class BuilderConfiguration
    {
        public INamedTypeSymbol? GlobalOptions { get; set; }

        public INamedTypeSymbol? ExitCode { get; set; }

        public bool IsKnown { get; set; } = true;
    }
}
