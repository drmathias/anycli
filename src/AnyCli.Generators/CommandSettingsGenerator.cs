// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AnyCli.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class CommandSettingsGenerator : IIncrementalGenerator
{
    private const string ArgumentAttributeName = "AnyCli.ArgumentAttribute";
    private const string VariadicArgumentAttributeName = "AnyCli.VariadicArgumentAttribute";
    private const string SettingsInterfaceName = "ICommandSettings`1";
    private const string ArgumentParserAttributeName = "ArgumentParserAttribute`2";
    private const string ArgumentParserInterfaceName = "IArgumentParser`1";
    private const string ParsableInterfaceName = "IParsable`1";
    private const string SpanParsableInterfaceName = "ISpanParsable`1";

    private static readonly DiagnosticDescriptor UnsupportedArgumentTypeDiagnostic = new(
        id: "ANYCLI001",
        title: "Unsupported argument type",
        messageFormat:
            "Argument property '{0}' has unsupported type '{1}'; use a type that implements IParsable<T>, an enum, specify Parser = typeof(...), or register an ArgumentParserAttribute<{1}, TParser>",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidArgumentParserDiagnostic = new(
        id: "ANYCLI002",
        title: "Invalid argument parser",
        messageFormat:
            "Parser '{1}' for argument property '{0}' must implement IArgumentParser<{2}>",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidVariadicArgumentDiagnostic = new(
        id: "ANYCLI003",
        title: "Invalid variadic argument",
        messageFormat:
            "Variadic argument property '{0}' must be a single-dimensional array; actual type '{1}'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateArgumentParserDiagnostic = new(
        id: "ANYCLI004",
        title: "Duplicate argument parser registration",
        messageFormat: "Multiple argument parsers are registered for type '{0}'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsCandidate(node),
                static (syntaxContext, cancellationToken) => GetModel(syntaxContext, cancellationToken))
            .Where(static model => model is not null);

        var registrations = context.CompilationProvider.Select(
            static (compilation, cancellationToken) =>
                GetArgumentParserRegistrations(compilation, cancellationToken));

        context.RegisterSourceOutput(
            models,
            static (productionContext, model) => Emit(productionContext, model!));

        context.RegisterSourceOutput(
            registrations,
            static (productionContext, registrationSet) =>
                ReportRegistrationDiagnostics(productionContext, registrationSet));
    }

    private static bool IsCandidate(SyntaxNode node) =>
        node is ClassDeclarationSyntax declaration &&
        declaration.Modifiers.Any(SyntaxKind.PartialKeyword) &&
        declaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Any(property => property.AttributeLists.Count > 0);

    private static SettingsModel? GetModel(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var declaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken);
        if (symbol is not INamedTypeSymbol type ||
            type.TypeKind != TypeKind.Class ||
            type.IsStatic ||
            type.IsAbstract ||
            type.Arity > 0 ||
            type.ContainingType is not null)
        {
            return null;
        }

        var registrations = GetArgumentParserRegistrations(
            context.SemanticModel.Compilation,
            cancellationToken);
        var arguments = new List<ArgumentModel>();
        var diagnostics = new List<DiagnosticModel>();
        foreach (var property in type.GetMembers().OfType<IPropertySymbol>())
        {
            if (property.SetMethod is null || property.SetMethod.IsInitOnly)
            {
                return null;
            }

            foreach (var attribute in property.GetAttributes())
            {
                var attributeName = attribute.AttributeClass?.ToDisplayString();
                var isVariadic = attributeName == VariadicArgumentAttributeName;
                if (!isVariadic && attributeName != ArgumentAttributeName)
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 0 ||
                    attribute.ConstructorArguments[0].Value is not int position)
                {
                    return null;
                }

                var name = ToCamelCase(property.Name);
                var description = string.Empty;
                var optional = false;
                ITypeSymbol? parserType = null;
                foreach (var namedArgument in attribute.NamedArguments)
                {
                    if (namedArgument.Key == "Name" && namedArgument.Value.Value is string customName)
                    {
                        name = customName;
                    }
                    else if (namedArgument.Key == "Description" &&
                             namedArgument.Value.Value is string customDescription)
                    {
                        description = customDescription;
                    }
                    else if (namedArgument.Key == "Optional" && namedArgument.Value.Value is bool isOptional)
                    {
                        optional = isOptional;
                    }
                    else if (namedArgument.Key == "Parser" &&
                             namedArgument.Value.Value is ITypeSymbol customParserType)
                    {
                        parserType = customParserType;
                    }
                }

                ITypeSymbol argumentType;
                ITypeSymbol valueType;
                string elementTypeFullName;
                if (isVariadic)
                {
                    if (property.Type is not IArrayTypeSymbol { Rank: 1 } array)
                    {
                        diagnostics.Add(new DiagnosticModel(
                            InvalidVariadicArgumentDiagnostic,
                            GetLocation(property),
                            property.Name,
                            property.Type.ToDisplayString()));
                        continue;
                    }

                    argumentType = array.ElementType;
                    valueType = UnwrapNullable(array.ElementType);
                    elementTypeFullName = GetTypeName(array.ElementType);
                    optional = false;
                }
                else
                {
                    argumentType = property.Type;
                    valueType = UnwrapNullable(property.Type);
                    elementTypeFullName = GetTypeName(property.Type);
                }

                if (!TryCreateParsePlan(argumentType, valueType, parserType, registrations.Parsers, out var parsePlan))
                {
                    diagnostics.Add(parserType is null
                        ? new DiagnosticModel(
                            UnsupportedArgumentTypeDiagnostic,
                            GetLocation(property),
                            property.Name,
                            argumentType.ToDisplayString())
                        : new DiagnosticModel(
                            InvalidArgumentParserDiagnostic,
                            GetLocation(property),
                            property.Name,
                            parserType.ToDisplayString(),
                            valueType.ToDisplayString()));
                    continue;
                }

                arguments.Add(new ArgumentModel(
                    name,
                    description,
                    property.Name,
                    position,
                    optional,
                    isVariadic,
                    elementTypeFullName,
                    parsePlan));
            }
        }

        if (arguments.Count == 0 && diagnostics.Count == 0)
        {
            return null;
        }

        return new SettingsModel(
            type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace.ToDisplayString(),
            type.Name,
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            GetAccessibility(type.DeclaredAccessibility),
            type.IsSealed,
            type.IsRecord,
            ImplementsSettingsInterface(type),
            arguments.OrderBy(argument => argument.Position).ToArray(),
            diagnostics.ToArray());
    }

    private static Location GetLocation(IPropertySymbol property) =>
        property.Locations.FirstOrDefault() ?? Location.None;

    private static string GetTypeName(ITypeSymbol type) =>
        type.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named &&
            named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
            named.TypeArguments.Length == 1)
        {
            return named.TypeArguments[0];
        }

        return type;
    }

    private static bool TryCreateParsePlan(
        ITypeSymbol argumentType,
        ITypeSymbol valueType,
        ITypeSymbol? parserType,
        IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> registeredParsers,
        out ParsePlan parsePlan)
    {
        if (parserType is not null)
        {
            if (!IsValidArgumentParser(parserType, valueType))
            {
                parsePlan = null!;
                return false;
            }

            parsePlan = new ParsePlan(
                ParseKind.Custom,
                valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                parserType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            return true;
        }

        if (registeredParsers.TryGetValue(valueType, out var registeredParser))
        {
            parsePlan = new ParsePlan(
                ParseKind.Custom,
                valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                registeredParser.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            return true;
        }

        if (argumentType.SpecialType == SpecialType.System_String)
        {
            parsePlan = new ParsePlan(
                ParseKind.String,
                argumentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                null);
            return true;
        }

        if (valueType.TypeKind == TypeKind.Enum)
        {
            parsePlan = new ParsePlan(
                ParseKind.Enum,
                valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                null);
            return true;
        }

        if (ImplementsGenericInterface(valueType, SpanParsableInterfaceName))
        {
            parsePlan = new ParsePlan(
                ParseKind.SpanParsable,
                valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                null);
            return true;
        }

        if (ImplementsGenericInterface(valueType, ParsableInterfaceName))
        {
            parsePlan = new ParsePlan(
                ParseKind.Parsable,
                valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                null);
            return true;
        }

        parsePlan = null!;
        return false;
    }

    private static bool IsValidArgumentParser(ITypeSymbol parserType, ITypeSymbol valueType)
    {
        if (parserType is not INamedTypeSymbol { IsStatic: false, IsUnboundGenericType: false } parser)
        {
            return false;
        }

        return parser.AllInterfaces.Any(
            currentInterface =>
                currentInterface.OriginalDefinition.MetadataName == ArgumentParserInterfaceName &&
                currentInterface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "AnyCli" &&
                currentInterface.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(currentInterface.TypeArguments[0], valueType));
    }

    private static bool ImplementsGenericInterface(ITypeSymbol type, string metadataName) =>
        type.AllInterfaces.Any(
            currentInterface =>
                currentInterface.OriginalDefinition.MetadataName == metadataName &&
                currentInterface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System" &&
                currentInterface.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(currentInterface.TypeArguments[0], type));

    private static RegistrationSet GetArgumentParserRegistrations(
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var parsers = new Dictionary<ITypeSymbol, ITypeSymbol>(SymbolEqualityComparer.Default);
        var diagnostics = new List<DiagnosticModel>();

        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null ||
                !attributeClass.IsGenericType ||
                attributeClass.OriginalDefinition.MetadataName != ArgumentParserAttributeName ||
                attributeClass.OriginalDefinition.ContainingNamespace.ToDisplayString() != "AnyCli" ||
                attributeClass.TypeArguments.Length != 2)
            {
                continue;
            }

            var valueType = attributeClass.TypeArguments[0];
            var parserType = attributeClass.TypeArguments[1];
            if (parsers.ContainsKey(valueType))
            {
                diagnostics.Add(new DiagnosticModel(
                    DuplicateArgumentParserDiagnostic,
                    GetLocation(attribute, cancellationToken),
                    valueType.ToDisplayString()));
                continue;
            }

            parsers[valueType] = parserType;
        }

        return new RegistrationSet(parsers, diagnostics);
    }

    private static Location GetLocation(AttributeData attribute, CancellationToken cancellationToken) =>
        attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken).GetLocation() ?? Location.None;

    private static void ReportRegistrationDiagnostics(
        SourceProductionContext context,
        RegistrationSet registrations)
    {
        foreach (var diagnostic in registrations.Diagnostics)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                diagnostic.Descriptor,
                diagnostic.Location,
                diagnostic.MessageArgs));
        }
    }

    private static string ToCamelCase(string value)
    {
        if (value.Length == 0 || char.IsLower(value[0]))
        {
            return value;
        }

        var characters = value.ToCharArray();
        characters[0] = char.ToLowerInvariant(characters[0]);
        return new string(characters);
    }

    private static bool ImplementsSettingsInterface(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(
            currentInterface =>
                currentInterface.OriginalDefinition.MetadataName == SettingsInterfaceName &&
                currentInterface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "AnyCli");

    private static string GetAccessibility(Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "internal",
        };

    private static void Emit(SourceProductionContext context, SettingsModel model)
    {
        if (model.Diagnostics.Count > 0)
        {
            foreach (var diagnostic in model.Diagnostics)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    diagnostic.Descriptor,
                    diagnostic.Location,
                    diagnostic.MessageArgs));
            }

            return;
        }

        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");
        source.AppendLine();

        var hasNamespace = model.Namespace.Length > 0;
        if (hasNamespace)
        {
            source.Append("namespace ").Append(model.Namespace).AppendLine();
            source.AppendLine("{");
        }

        var indent = hasNamespace ? "    " : string.Empty;
        source.Append(indent)
            .Append(model.Accessibility)
            .Append(' ');

        if (model.IsSealed)
        {
            source.Append("sealed ");
        }

        source.Append("partial ").Append(model.IsRecord ? "record" : "class").Append(' ').Append(model.TypeName);

        if (!model.ImplementsSettingsInterface)
        {
            source.Append(" : global::AnyCli.ICommandSettings<").Append(model.FullTypeName).Append('>');
        }

        source.AppendLine();
        source.Append(indent).AppendLine("{");

        source.Append(indent).AppendLine("    public static void Configure(global::AnyCli.CommandArgumentBuilder arguments)");
        source.Append(indent).AppendLine("    {");
        foreach (var argument in model.Arguments)
        {
            source.Append(indent).Append("        arguments.");
            if (argument.Variadic)
            {
                source.Append("AddVariadic");
            }
            else if (argument.Optional)
            {
                source.Append("AddOptional");
            }
            else
            {
                source.Append("Add");
            }

            source.Append('<')
                .Append(argument.ElementTypeFullName)
                .Append(">(")
                .Append(SymbolDisplay.FormatLiteral(argument.Name, true))
                .AppendLine(");");

            if (argument.Description.Length > 0)
            {
                source.Append(indent)
                    .Append("        arguments.Describe(")
                    .Append(SymbolDisplay.FormatLiteral(argument.Description, true))
                    .AppendLine(");");
            }
        }

        source.Append(indent).AppendLine("    }");
        source.AppendLine();

        source.Append(indent)
            .Append("    public static ")
            .Append(model.FullTypeName)
            .AppendLine(" Bind(global::AnyCli.CommandArguments arguments)");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).Append("        var settings = new ").Append(model.FullTypeName).AppendLine("();");

        var localIndex = 0;
        foreach (var argument in model.Arguments)
        {
            var literalName = SymbolDisplay.FormatLiteral(argument.Name, true);
            if (argument.Variadic)
            {
                if (argument.ParsePlan.Kind == ParseKind.String)
                {
                    source.Append(indent)
                        .Append("        settings.")
                        .Append(argument.PropertyName)
                        .Append(" = global::System.Linq.Enumerable.ToArray(arguments.GetVariadic(")
                        .Append(literalName)
                        .AppendLine("));");
                }
                else
                {
                    var rawLocal = "__raw" + localIndex;
                    var valuesLocal = "__values" + localIndex;
                    var indexLocal = "__i" + localIndex;
                    source.Append(indent)
                        .Append("        var ")
                        .Append(rawLocal)
                        .Append(" = arguments.GetVariadic(")
                        .Append(literalName)
                        .AppendLine(");");
                    source.Append(indent)
                        .Append("        var ")
                        .Append(valuesLocal)
                        .Append(" = new ")
                        .Append(argument.ElementTypeFullName)
                        .Append('[')
                        .Append(rawLocal)
                        .AppendLine(".Count];");
                    source.Append(indent)
                        .Append("        for (var ")
                        .Append(indexLocal)
                        .Append(" = 0; ")
                        .Append(indexLocal)
                        .Append(" < ")
                        .Append(rawLocal)
                        .Append(".Count; ")
                        .Append(indexLocal)
                        .AppendLine("++)");
                    source.Append(indent).AppendLine("        {");
                    source.Append(indent)
                        .Append("            ")
                        .Append(valuesLocal)
                        .Append('[')
                        .Append(indexLocal)
                        .Append("] = ")
                        .Append(GetParseExpression(
                            argument,
                            rawLocal + "[" + indexLocal + "]",
                            literalName))
                        .AppendLine(";");
                    source.Append(indent).AppendLine("        }");
                    source.Append(indent)
                        .Append("        settings.")
                        .Append(argument.PropertyName)
                        .Append(" = ")
                        .Append(valuesLocal)
                        .AppendLine(";");
                }
            }
            else if (argument.Optional)
            {
                var rawLocal = "__raw" + localIndex;
                source.Append(indent)
                    .Append("        if (arguments.TryGetOptional(")
                    .Append(literalName)
                    .Append(", out var ")
                    .Append(rawLocal)
                    .AppendLine("))");
                source.Append(indent).AppendLine("        {");
                source.Append(indent)
                    .Append("            settings.")
                    .Append(argument.PropertyName)
                    .Append(" = ")
                    .Append(GetParseExpression(argument, rawLocal + "!", literalName))
                    .AppendLine(";");
                source.Append(indent).AppendLine("        }");
            }
            else
            {
                source.Append(indent)
                    .Append("        settings.")
                    .Append(argument.PropertyName)
                    .Append(" = ")
                    .Append(GetParseExpression(
                        argument,
                        "arguments.GetRequired(" + literalName + ")",
                        literalName))
                    .AppendLine(";");
            }

            localIndex++;
        }

        source.Append(indent).AppendLine("        return settings;");
        source.Append(indent).AppendLine("    }");
        source.Append(indent).AppendLine("}");

        if (hasNamespace)
        {
            source.AppendLine("}");
        }

        context.AddSource(
            model.FullTypeName.Replace("global::", string.Empty).Replace('.', '_') + ".CommandSettings.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static string GetParseExpression(
        ArgumentModel argument,
        string rawExpression,
        string literalName) =>
        argument.ParsePlan.Kind switch
        {
            ParseKind.String => rawExpression,
            ParseKind.Enum =>
                $"arguments.ParseEnum<{argument.ParsePlan.ValueTypeFullName}>(" +
                $"{rawExpression}, {literalName})",
            ParseKind.SpanParsable =>
                $"global::AnyCli.ArgumentParsing.ParseSpanParsable<{argument.ParsePlan.ValueTypeFullName}>(" +
                $"{rawExpression}, global::System.Globalization.CultureInfo.InvariantCulture, {literalName})",
            ParseKind.Parsable =>
                $"global::AnyCli.ArgumentParsing.ParseParsable<{argument.ParsePlan.ValueTypeFullName}>(" +
                $"{rawExpression}, global::System.Globalization.CultureInfo.InvariantCulture, {literalName})",
            ParseKind.Custom =>
                $"global::AnyCli.ArgumentParsing.ParseCustom<{argument.ParsePlan.ValueTypeFullName}, " +
                $"{argument.ParsePlan.ParserTypeFullName}>({rawExpression}, " +
                $"global::System.Globalization.CultureInfo.InvariantCulture, {literalName})",
            _ => throw new InvalidOperationException("Unknown parse kind."),
        };

    private sealed class SettingsModel
    {
        public SettingsModel(
            string @namespace,
            string typeName,
            string fullTypeName,
            string accessibility,
            bool isSealed,
            bool isRecord,
            bool implementsSettingsInterface,
            IReadOnlyList<ArgumentModel> arguments,
            IReadOnlyList<DiagnosticModel> diagnostics)
        {
            Namespace = @namespace;
            TypeName = typeName;
            FullTypeName = fullTypeName;
            Accessibility = accessibility;
            IsSealed = isSealed;
            IsRecord = isRecord;
            ImplementsSettingsInterface = implementsSettingsInterface;
            Arguments = arguments;
            Diagnostics = diagnostics;
        }

        public string Namespace { get; }

        public string TypeName { get; }

        public string FullTypeName { get; }

        public string Accessibility { get; }

        public bool IsSealed { get; }

        public bool IsRecord { get; }

        public bool ImplementsSettingsInterface { get; }

        public IReadOnlyList<ArgumentModel> Arguments { get; }

        public IReadOnlyList<DiagnosticModel> Diagnostics { get; }
    }

    private sealed class ArgumentModel
    {
        public ArgumentModel(
            string name,
            string description,
            string propertyName,
            int position,
            bool optional,
            bool variadic,
            string elementTypeFullName,
            ParsePlan parsePlan)
        {
            Name = name;
            Description = description;
            PropertyName = propertyName;
            Position = position;
            Optional = optional;
            Variadic = variadic;
            ElementTypeFullName = elementTypeFullName;
            ParsePlan = parsePlan;
        }

        public string Name { get; }

        public string Description { get; }

        public string PropertyName { get; }

        public int Position { get; }

        public bool Optional { get; }

        public bool Variadic { get; }

        public string ElementTypeFullName { get; }

        public ParsePlan ParsePlan { get; }
    }

    private sealed class RegistrationSet(
        IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> parsers,
        IReadOnlyList<DiagnosticModel> diagnostics)
    {
        public IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> Parsers { get; } = parsers;

        public IReadOnlyList<DiagnosticModel> Diagnostics { get; } = diagnostics;
    }

    private sealed class ParsePlan(
        ParseKind kind,
        string valueTypeFullName,
        string? parserTypeFullName)
    {
        public ParseKind Kind { get; } = kind;

        public string ValueTypeFullName { get; } = valueTypeFullName;

        public string? ParserTypeFullName { get; } = parserTypeFullName;
    }

    private sealed class DiagnosticModel(
        DiagnosticDescriptor descriptor,
        Location location,
        params object?[] messageArgs)
    {
        public DiagnosticDescriptor Descriptor { get; } = descriptor;

        public Location Location { get; } = location;

        public object?[] MessageArgs { get; } = messageArgs;
    }

    private enum ParseKind
    {
        String,
        Enum,
        SpanParsable,
        Parsable,
        Custom,
    }
}
