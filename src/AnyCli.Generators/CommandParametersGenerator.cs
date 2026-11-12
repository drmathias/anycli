// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Immutable;
using System.Globalization;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AnyCli.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class CommandParametersGenerator : IIncrementalGenerator
{
    private const string ArgumentAttributeName = "AnyCli.Parameters.Arguments.ArgumentAttribute";
    private const string OptionalArgumentAttributeName = "AnyCli.Parameters.Arguments.OptionalArgumentAttribute";
    private const string VariadicArgumentAttributeName = "AnyCli.Parameters.Arguments.VariadicArgumentAttribute";
    private const string OptionAttributeName = "AnyCli.Parameters.Options.OptionAttribute";
    private const string RequiredOptionAttributeName = "AnyCli.Parameters.Options.RequiredOptionAttribute";
    private const string GlobalOptionsAttributeName = "AnyCli.Parameters.Options.GlobalOptionsAttribute";
    private const string ParametersInterfaceName = "ICommandParameters`1";
    private const string GlobalOptionsInterfaceName = "IGlobalOptions`1";
    private const string ParameterValueParserAttributeName = "ParameterValueParserAttribute`2";
    private const string EnumParameterParserAttributeName = "EnumParameterParserAttribute`1";
    private const string ParameterParserInterfaceName = "IParameterParser`1";
    private const string ParsableInterfaceName = "IParsable`1";
    private const string SpanParsableInterfaceName = "ISpanParsable`1";

    private static readonly DiagnosticDescriptor UnsupportedParameterTypeDiagnostic = new(
        id: "ANYCLI001",
        title: "Unsupported parameter type",
        messageFormat:
            "Parameter property '{0}' has unsupported type '{1}'; use a type that implements IParsable<T>, an enum, specify Parser = typeof(...), or register a ParameterValueParserAttribute<{1}, TParser>",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidParameterParserDiagnostic = new(
        id: "ANYCLI002",
        title: "Invalid parameter parser",
        messageFormat:
            "Parser '{1}' for parameter property '{0}' must implement IParameterParser<{2}>",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidVariadicArgumentDiagnostic = new(
        id: "ANYCLI003",
        title: "Invalid variadic parameter",
        messageFormat:
            "Variadic parameter property '{0}' must be a supported collection type; actual type '{1}'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateParameterParserDiagnostic = new(
        id: "ANYCLI004",
        title: "Duplicate parameter parser registration",
        messageFormat: "Multiple parameter parsers are registered for type '{0}'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidDefaultValueTypeDiagnostic = new(
        id: "ANYCLI006",
        title: "Invalid default value type",
        messageFormat:
            "Default value for parameter property '{0}' cannot be assigned to type '{1}'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor UnsupportedDefaultValueDiagnostic = new(
        id: "ANYCLI008",
        title: "Unsupported default value",
        messageFormat:
            "Default value for parameter property '{0}' is not a supported constant",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidOptionNameDiagnostic = new(
        id: "ANYCLI009",
        title: "Invalid option name",
        messageFormat:
            "Option property '{0}' has invalid option name '{1}'; use a short name such as '-o' or a long name such as '--output'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidArgumentPositionDiagnostic = new(
        id: "ANYCLI007",
        title: "Invalid argument position",
        messageFormat:
            "Argument property '{0}' has invalid position {1}; argument positions must be unique, non-negative, and contiguous starting at 0",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidOptionAliasDiagnostic = new(
        id: "ANYCLI013",
        title: "Invalid option alias",
        messageFormat:
            "Option property '{0}' has invalid alias '{1}'; use a short name such as '-o' or a long name such as '--output'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateOptionNameDiagnostic = new(
        id: "ANYCLI014",
        title: "Duplicate option name",
        messageFormat:
            "Option property '{0}' uses option name or alias '{1}' more than once",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidCollectionDefaultValueDiagnostic = new(
        id: "ANYCLI010",
        title: "Invalid collection default value",
        messageFormat:
            "DefaultValue for collection parameter property '{0}' must be a non-null array",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidDefaultValueElementTypeDiagnostic = new(
        id: "ANYCLI011",
        title: "Invalid default value element type",
        messageFormat:
            "DefaultValue for parameter property '{0}' contains a value that cannot be assigned to element type '{1}'",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor RequiredVariadicDefaultValueDiagnostic = new(
        id: "ANYCLI012",
        title: "Default values on required variadic parameter",
        messageFormat:
            "Variadic parameter property '{0}' with MinimumCount {1} cannot have a default value",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidVariadicMinimumCountDiagnostic = new(
        id: "ANYCLI005",
        title: "Invalid variadic minimum count",
        messageFormat:
            "Variadic parameter property '{0}' has MinimumCount {1}; it must be non-negative",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor InvalidVariadicMaximumCountDiagnostic = new(
        id: "ANYCLI017",
        title: "Invalid variadic maximum count",
        messageFormat:
            "Variadic parameter property '{0}' has MaximumCount {1}; it must be greater than 1 and at least MinimumCount {2}",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor GlobalOptionCannotBeArgumentDiagnostic = new(
        id: "ANYCLI015",
        title: "Global option declared as argument",
        messageFormat:
            "Global option property '{0}' cannot be declared as a positional argument",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ConflictingParametersKindDiagnostic = new(
        id: "ANYCLI016",
        title: "Conflicting parameters kind",
        messageFormat:
            "Type '{0}' cannot be both global options and command parameters",
        category: "AnyCli",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsCandidate(node),
                static (syntaxContext, cancellationToken) => GetModel(syntaxContext, cancellationToken))
            .Where(static model => model is not null)
            .Collect()
            .SelectMany(static (models, _) => Deduplicate(models));

        var registrations = context.CompilationProvider.Select(
            static (compilation, cancellationToken) =>
                GetParameterParserRegistrations(compilation, cancellationToken));

        context.RegisterSourceOutput(
            models,
            static (productionContext, model) => Emit(productionContext, model!));

        context.RegisterSourceOutput(
            registrations,
            static (productionContext, registrationSet) =>
                ReportRegistrationDiagnostics(productionContext, registrationSet));
    }

    private static IEnumerable<ParametersModel> Deduplicate(
        ImmutableArray<ParametersModel?> models)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var model in models)
        {
            if (model is not null && seen.Add(model.FullTypeName))
            {
                yield return model;
            }
        }
    }

    private static bool IsCandidate(SyntaxNode node) =>
        node is TypeDeclarationSyntax declaration &&
        declaration is ClassDeclarationSyntax or RecordDeclarationSyntax &&
        declaration.Modifiers.Any(SyntaxKind.PartialKeyword) &&
        (declaration.BaseList is not null ||
         declaration.Members
             .OfType<PropertyDeclarationSyntax>()
             .Any(property => property.AttributeLists.Count > 0) ||
         HasGlobalOptionsAttributeSyntax(declaration));

    private static bool HasGlobalOptionsAttributeSyntax(TypeDeclarationSyntax declaration) =>
        declaration.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute =>
            {
                var name = attribute.Name switch
                {
                    QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
                    AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText,
                    SimpleNameSyntax simple => simple.Identifier.ValueText,
                    _ => string.Empty,
                };

                return name is "GlobalOptions" or "GlobalOptionsAttribute";
            });

    private static ParametersModel? GetModel(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken)
    {
        var declaration = (TypeDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(declaration, cancellationToken);
        if (symbol is not INamedTypeSymbol type)
        {
            return null;
        }

        var isGlobalOptions = HasGlobalOptionsAttribute(type);
        if (type.TypeKind != TypeKind.Class ||
            type.IsStatic ||
            type.IsAbstract ||
            type.Arity > 0 ||
            type.ContainingType is not null)
        {
            return null;
        }

        var compilation = context.SemanticModel.Compilation;
        if (!isGlobalOptions && ImplementsGlobalOptionsInterface(type))
        {
            return null;
        }

        var globalOptionsBase = isGlobalOptions ? null : GetGlobalOptionsBase(type);
        var registrations = GetParameterParserRegistrations(
            compilation,
            cancellationToken);
        var arguments = new List<ArgumentModel>();
        var argumentPositions = new List<(int Position, Location Location, string PropertyName)>();
        var options = new List<OptionModel>();
        var globalOptions = new List<OptionModel>();
        var optionNames = new HashSet<string>(StringComparer.Ordinal);
        var diagnostics = new List<DiagnosticModel>();
        if (isGlobalOptions && ImplementsParametersInterface(type))
        {
            diagnostics.Add(new DiagnosticModel(
                ConflictingParametersKindDiagnostic,
                type.Locations.FirstOrDefault() ?? Location.None,
                type.Name));
        }

        foreach (var property in GetParameterProperties(type, compilation))
        {
            if (property.SetMethod is null || property.SetMethod.IsInitOnly)
            {
                return null;
            }

            var isGlobalOptionProperty = globalOptionsBase is not null &&
                IsDeclaredInTypeOrBase(globalOptionsBase, property.ContainingType);

            foreach (var attribute in GetParameterAttributes(property))
            {
                var attributeName = attribute.AttributeClass?.ToDisplayString();
                if (attributeName == OptionAttributeName ||
                    attributeName == RequiredOptionAttributeName)
                {
                    var option = CreateOptionModel(
                        property,
                        attribute,
                        attributeName == RequiredOptionAttributeName,
                        registrations,
                        compilation,
                        diagnostics);
                    if (option is not null)
                    {
                        if (isGlobalOptionProperty)
                        {
                            globalOptions.Add(option);
                        }
                        else
                        {
                            options.Add(option);
                            foreach (var optionName in option.Names)
                            {
                                if (!optionNames.Add(optionName))
                                {
                                    diagnostics.Add(new DiagnosticModel(
                                        DuplicateOptionNameDiagnostic,
                                        GetLocation(property),
                                        property.Name,
                                        optionName));
                                }
                            }
                        }
                    }

                    continue;
                }

                if (isGlobalOptionProperty)
                {
                    continue;
                }

                var isOptionalArgument = attributeName == OptionalArgumentAttributeName;
                var isVariadic = attributeName == VariadicArgumentAttributeName;
                if (!isVariadic && !isOptionalArgument && attributeName != ArgumentAttributeName)
                {
                    continue;
                }

                if (isGlobalOptions)
                {
                    diagnostics.Add(new DiagnosticModel(
                        GlobalOptionCannotBeArgumentDiagnostic,
                        GetLocation(property),
                        property.Name));
                    continue;
                }

                if (attribute.ConstructorArguments.Length == 0 ||
                    attribute.ConstructorArguments[0].Value is not int position)
                {
                    return null;
                }

                var location = GetLocation(property);
                if (position < 0 ||
                    argumentPositions.Any(existing => existing.Position == position))
                {
                    diagnostics.Add(new DiagnosticModel(
                        InvalidArgumentPositionDiagnostic,
                        location,
                        property.Name,
                        position));
                    continue;
                }

                argumentPositions.Add((position, location, property.Name));

                var name = ToCamelCase(property.Name);
                var description = string.Empty;
                var optional = isOptionalArgument;
                var minimumCount = 0;
                var maximumCount = int.MaxValue;
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
                    else if (namedArgument.Key == "Parser" &&
                             namedArgument.Value.Value is ITypeSymbol customParserType)
                    {
                        parserType = customParserType;
                    }
                    else if (namedArgument.Key == "MinimumCount" &&
                             namedArgument.Value.Value is int customMinimumCount)
                    {
                        minimumCount = customMinimumCount;
                    }
                    else if (namedArgument.Key == "MaximumCount" &&
                             namedArgument.Value.Value is int customMaximumCount)
                    {
                        maximumCount = customMaximumCount;
                    }
                }

                ITypeSymbol argumentType;
                ITypeSymbol valueType;
                string elementTypeFullName;
                string collectionTypeFullName;
                var collectionKind = VariadicCollectionKind.Array;
                if (isVariadic)
                {
                    if (!TryGetVariadicCollection(property.Type, out var elementType, out collectionKind))
                    {
                        diagnostics.Add(new DiagnosticModel(
                            InvalidVariadicArgumentDiagnostic,
                            GetLocation(property),
                            property.Name,
                            property.Type.ToDisplayString()));
                        continue;
                    }

                    if (minimumCount < 0)
                    {
                        diagnostics.Add(new DiagnosticModel(
                            InvalidVariadicMinimumCountDiagnostic,
                            GetLocation(property),
                            property.Name,
                            minimumCount));
                        continue;
                    }

                    if (maximumCount <= 1 || maximumCount < minimumCount)
                    {
                        diagnostics.Add(new DiagnosticModel(
                            InvalidVariadicMaximumCountDiagnostic,
                            GetLocation(property),
                            property.Name,
                            maximumCount,
                            minimumCount));
                        continue;
                    }

                    argumentType = elementType;
                    valueType = UnwrapNullable(elementType);
                    elementTypeFullName = GetTypeName(elementType);
                    collectionTypeFullName = GetTypeName(property.Type);
                    optional = false;
                }
                else
                {
                    argumentType = property.Type;
                    valueType = UnwrapNullable(property.Type);
                    elementTypeFullName = GetTypeName(property.Type);
                    collectionTypeFullName = GetTypeName(property.Type);
                }

                string? defaultExpression = null;
                if (isOptionalArgument && !TryGetDefaultValueExpression(
                        property,
                        attribute,
                        property.Type,
                        isCollection: false,
                        elementType: null,
                        isVariadic: false,
                        minimumCount: 0,
                        compilation,
                        diagnostics,
                        out defaultExpression))
                {
                    continue;
                }

                if (isVariadic)
                {
                    _ = TryGetVariadicCollection(
                        property.Type,
                        out var defaultElementType,
                        out _);

                    if (!TryGetDefaultValueExpression(
                            property,
                            attribute,
                            property.Type,
                            isCollection: true,
                            defaultElementType,
                            isVariadic: true,
                            minimumCount,
                            compilation,
                            diagnostics,
                            out defaultExpression))
                    {
                        continue;
                    }
                }

                if (!TryCreateParsePlan(
                    argumentType,
                    valueType,
                    parserType,
                    registrations.Parsers,
                    registrations.EnumParser,
                    out var parsePlan))
                {
                    diagnostics.Add(parserType is null
                        ? new DiagnosticModel(
                            UnsupportedParameterTypeDiagnostic,
                            GetLocation(property),
                            property.Name,
                            argumentType.ToDisplayString())
                        : new DiagnosticModel(
                            InvalidParameterParserDiagnostic,
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
                    collectionTypeFullName,
                    collectionKind,
                    minimumCount,
                    maximumCount,
                    defaultExpression,
                    parsePlan));
            }
        }

        ValidateArgumentPositions(argumentPositions, diagnostics);

        return !isGlobalOptions &&
               arguments.Count == 0 &&
               options.Count == 0 &&
               globalOptions.Count == 0 &&
               diagnostics.Count == 0
            ? null
            : new ParametersModel(
                type.ContainingNamespace.IsGlobalNamespace ? string.Empty : type.ContainingNamespace.ToDisplayString(),
                type.Name,
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                GetAccessibility(type.DeclaredAccessibility),
                type.IsSealed,
                type.IsRecord,
                isGlobalOptions,
                ImplementsGlobalOptionsInterface(type),
                ImplementsParametersInterface(type),
                [.. arguments.OrderBy(argument => argument.Position)],
                [.. options],
                [.. globalOptions],
                [.. diagnostics]);
    }

    private static void ValidateArgumentPositions(
        IReadOnlyList<(int Position, Location Location, string PropertyName)> positions,
        List<DiagnosticModel> diagnostics)
    {
        var expectedPosition = 0;
        foreach (var (position, location, propertyName) in positions.OrderBy(item => item.Position))
        {
            if (position != expectedPosition)
            {
                diagnostics.Add(new DiagnosticModel(
                    InvalidArgumentPositionDiagnostic,
                    location,
                    propertyName,
                    position));
                return;
            }

            expectedPosition++;
        }
    }

    private static IReadOnlyList<AttributeData> GetParameterAttributes(IPropertySymbol property)
    {
        for (var current = property; current is not null; current = current.OverriddenProperty)
        {
            var attributes = current.GetAttributes()
                .Where(attribute => IsParameterAttribute(attribute.AttributeClass))
                .ToArray();
            if (attributes.Length > 0)
            {
                return attributes;
            }
        }

        return [];
    }

    private static bool IsParameterAttribute(INamedTypeSymbol? attributeClass)
    {
        var attributeName = attributeClass?.ToDisplayString();
        return attributeName == OptionAttributeName ||
               attributeName == RequiredOptionAttributeName ||
               attributeName == ArgumentAttributeName ||
               attributeName == OptionalArgumentAttributeName ||
               attributeName == VariadicArgumentAttributeName;
    }

    private static IEnumerable<IPropertySymbol> GetParameterProperties(
        INamedTypeSymbol type,
        Compilation compilation)
    {
        var hierarchy = new Stack<INamedTypeSymbol>();
        for (var current = type;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            hierarchy.Push(current);
        }

        var properties = new List<IPropertySymbol>();
        var propertyIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var current in hierarchy)
        {
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic ||
                    property.IsIndexer ||
                    property.SetMethod is null ||
                    !compilation.IsSymbolAccessibleWithin(property, type, type) ||
                    !compilation.IsSymbolAccessibleWithin(property.SetMethod, type, type))
                {
                    continue;
                }

                if (propertyIndexes.TryGetValue(property.Name, out var existingIndex))
                {
                    properties[existingIndex] = property;
                }
                else
                {
                    propertyIndexes.Add(property.Name, properties.Count);
                    properties.Add(property);
                }
            }
        }

        return properties;
    }

    private static OptionModel? CreateOptionModel(
        IPropertySymbol property,
        AttributeData attribute,
        bool required,
        RegistrationSet registrations,
        Compilation compilation,
        List<DiagnosticModel> diagnostics)
    {
        var name = "--" + ToKebabCase(property.Name);
        if (attribute.ConstructorArguments.Length > 0)
        {
            var explicitName = attribute.ConstructorArguments[0].Value as string;
            if (!TryNormalizeOptionName(explicitName, out name))
            {
                diagnostics.Add(new DiagnosticModel(
                    InvalidOptionNameDiagnostic,
                    GetLocation(property),
                    property.Name,
                    explicitName ?? "<null>"));
                return null;
            }
        }

        var aliases = new List<string>();
        var hasInvalidAlias = false;
        for (var argumentIndex = 1; argumentIndex < attribute.ConstructorArguments.Length; argumentIndex++)
        {
            var argument = attribute.ConstructorArguments[argumentIndex];
            if (argument.Kind == TypedConstantKind.Array)
            {
                if (argument.IsNull)
                {
                    diagnostics.Add(new DiagnosticModel(
                        InvalidOptionAliasDiagnostic,
                        GetLocation(property),
                        property.Name,
                        "<null>"));
                    hasInvalidAlias = true;
                    continue;
                }

                foreach (var aliasValue in argument.Values)
                {
                    if (aliasValue.Value is string alias &&
                        TryNormalizeOptionName(alias, out var normalizedAlias))
                    {
                        aliases.Add(normalizedAlias);
                    }
                    else
                    {
                        diagnostics.Add(new DiagnosticModel(
                            InvalidOptionAliasDiagnostic,
                            GetLocation(property),
                            property.Name,
                            aliasValue.Value as string ?? "<null>"));
                        hasInvalidAlias = true;
                    }
                }
            }
            else if (argument.Value is string alias &&
                     TryNormalizeOptionName(alias, out var normalizedAlias))
            {
                aliases.Add(normalizedAlias);
            }
            else
            {
                diagnostics.Add(new DiagnosticModel(
                    InvalidOptionAliasDiagnostic,
                    GetLocation(property),
                    property.Name,
                    argument.Value as string ?? "<null>"));
                hasInvalidAlias = true;
            }
        }

        if (hasInvalidAlias)
        {
            return null;
        }

        var description = string.Empty;
        ITypeSymbol? parserType = null;
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key == "Description" &&
                namedArgument.Value.Value is string customDescription)
            {
                description = customDescription;
            }
            else if (namedArgument.Key == "Parser" &&
                     namedArgument.Value.Value is ITypeSymbol customParserType)
            {
                parserType = customParserType;
            }
        }

        var isCollection = TryGetVariadicCollection(
            property.Type,
            out var elementType,
            out var collectionKind);
        ITypeSymbol argumentType;
        ITypeSymbol valueType;
        string elementTypeFullName;
        bool isFlag;

        if (isCollection)
        {
            argumentType = elementType;
            valueType = UnwrapNullable(elementType);
            elementTypeFullName = GetTypeName(elementType);
            isFlag = false;
        }
        else
        {
            argumentType = property.Type;
            valueType = UnwrapNullable(property.Type);
            elementTypeFullName = GetTypeName(property.Type);
            isFlag = valueType.SpecialType == SpecialType.System_Boolean;
        }

        string? defaultExpression = null;
        if (!required && !TryGetDefaultValueExpression(
                property,
                attribute,
                property.Type,
                isCollection,
                isCollection ? elementType : null,
                isVariadic: false,
                minimumCount: 0,
                compilation,
                diagnostics,
                out defaultExpression))
        {
            return null;
        }

        if (!TryCreateParsePlan(
                argumentType,
                valueType,
                parserType,
                registrations.Parsers,
                registrations.EnumParser,
                out var parsePlan))
        {
            diagnostics.Add(parserType is null
                ? new DiagnosticModel(
                    UnsupportedParameterTypeDiagnostic,
                    GetLocation(property),
                    property.Name,
                    argumentType.ToDisplayString())
                : new DiagnosticModel(
                    InvalidParameterParserDiagnostic,
                    GetLocation(property),
                    property.Name,
                    parserType.ToDisplayString(),
                    valueType.ToDisplayString()));
            return null;
        }

        return new OptionModel(
            name,
            aliases,
            description,
            property.Name,
            required,
            isFlag,
            isCollection,
            elementTypeFullName,
            collectionKind,
            defaultExpression,
            parsePlan);
    }

    private static bool TryGetDefaultValueExpression(
        IPropertySymbol property,
        AttributeData attribute,
        ITypeSymbol targetType,
        bool isCollection,
        ITypeSymbol? elementType,
        bool isVariadic,
        int minimumCount,
        Compilation compilation,
        List<DiagnosticModel> diagnostics,
        out string? expression)
    {
        expression = null;

        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key != "DefaultValue")
            {
                continue;
            }

            if (isCollection)
            {
                if (isVariadic && minimumCount > 0)
                {
                    diagnostics.Add(new DiagnosticModel(
                        RequiredVariadicDefaultValueDiagnostic,
                        GetLocation(property),
                        property.Name,
                        minimumCount));
                    return false;
                }

                if (elementType is null ||
                    namedArgument.Value.IsNull ||
                    namedArgument.Value.Kind != TypedConstantKind.Array)
                {
                    diagnostics.Add(new DiagnosticModel(
                        InvalidCollectionDefaultValueDiagnostic,
                        GetLocation(property),
                        property.Name));
                    return false;
                }

                var elements = new List<string>(namedArgument.Value.Values.Length);
                foreach (var element in namedArgument.Value.Values)
                {
                    if (!TryFormatDefaultValue(element, out var elementExpression) ||
                        !IsAssignableDefaultValue(element, elementType, compilation))
                    {
                        diagnostics.Add(new DiagnosticModel(
                            InvalidDefaultValueElementTypeDiagnostic,
                            GetLocation(property),
                            property.Name,
                            elementType.ToDisplayString()));
                        return false;
                    }

                    elements.Add(elementExpression!);
                }

                expression = $"new {GetTypeName(elementType)}[] {{ " +
                    string.Join(", ", elements) + " }";
                return true;
            }

            if (!TryFormatDefaultValue(namedArgument.Value, out expression))
            {
                diagnostics.Add(new DiagnosticModel(
                    UnsupportedDefaultValueDiagnostic,
                    GetLocation(property),
                    property.Name));
                return false;
            }

            if (!IsAssignableDefaultValue(
                    namedArgument.Value,
                    targetType,
                    compilation))
            {
                diagnostics.Add(new DiagnosticModel(
                    InvalidDefaultValueTypeDiagnostic,
                    GetLocation(property),
                    property.Name,
                    targetType.ToDisplayString()));
                expression = null;
                return false;
            }

            return true;
        }

        return true;
    }

    private static bool IsAssignableDefaultValue(
        TypedConstant value,
        ITypeSymbol targetType,
        Compilation compilation)
    {
        if (value.IsNull)
        {
            return targetType.IsReferenceType ||
                (targetType is INamedTypeSymbol named &&
                    named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);
        }

        if (value.Type is not { } sourceType)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(targetType, sourceType))
        {
            return true;
        }

        var unwrappedTarget = UnwrapNullable(targetType);
        if (SymbolEqualityComparer.Default.Equals(unwrappedTarget, sourceType))
        {
            return true;
        }

        return compilation.ClassifyConversion(sourceType, targetType).IsImplicit;
    }

    private static bool TryFormatDefaultValue(
        TypedConstant value,
        out string? expression)
    {
        expression = null;

        if (value.IsNull)
        {
            expression = "null";
            return true;
        }

        if (value.Kind == TypedConstantKind.Enum)
        {
            var enumType = value.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (enumType is null || value.Value is null)
            {
                return false;
            }

            expression = $"({enumType})({FormatPrimitiveValue(value.Value)})";
            return true;
        }

        if (value.Kind == TypedConstantKind.Primitive)
        {
            expression = FormatPrimitiveValue(value.Value);
            return true;
        }

        if (value.Kind == TypedConstantKind.Type && value.Value is ITypeSymbol type)
        {
            expression = $"typeof({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";
            return true;
        }

        return false;
    }

    private static string FormatPrimitiveValue(object? value)
    {
        var actual = value;
        if (actual is not null && actual.GetType().IsEnum)
        {
            actual = Convert.ChangeType(
                actual,
                Enum.GetUnderlyingType(actual.GetType()),
                CultureInfo.InvariantCulture);
        }

        return SymbolDisplay.FormatPrimitive(
            actual!,
            quoteStrings: true,
            useHexadecimalNumbers: false);
    }

    private static bool TryNormalizeOptionName(string? name, out string normalizedName)
    {
        normalizedName = string.Empty;
        if (name is null ||
            string.IsNullOrWhiteSpace(name) ||
            !string.Equals(name, name.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        normalizedName = name[0] == '-'
            ? name
            : name.Length == 1
                ? "-" + name
                : "--" + name;

        if (normalizedName.Length < 2)
        {
            return false;
        }

        if (normalizedName[1] == '-')
        {
            return normalizedName.Length > 2 &&
                   IsValidLongOptionBody(normalizedName.Substring(2));
        }

        return normalizedName.Length == 2 &&
               char.IsLetterOrDigit(normalizedName[1]);
    }

    private static bool IsValidLongOptionBody(string body)
    {
        if (body.Length == 0 ||
            body[0] == '-' ||
            body[body.Length - 1] == '-')
        {
            return false;
        }

        var previousWasHyphen = false;
        foreach (var character in body)
        {
            if (character == '-')
            {
                if (previousWasHyphen)
                {
                    return false;
                }

                previousWasHyphen = true;
                continue;
            }

            if (!char.IsLetterOrDigit(character) && character != '_')
            {
                return false;
            }

            previousWasHyphen = false;
        }

        return true;
    }

    private static string ToKebabCase(string value)
    {
        var builder = new StringBuilder(value.Length + 4);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (index > 0 && char.IsUpper(character))
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        return builder.ToString();
    }

    private static Location GetLocation(IPropertySymbol property) =>
        property.Locations.FirstOrDefault() ?? Location.None;

    private static bool TryGetVariadicCollection(
        ITypeSymbol type,
        out ITypeSymbol elementType,
        out VariadicCollectionKind collectionKind)
    {
        if (type is IArrayTypeSymbol { Rank: 1 } array)
        {
            elementType = array.ElementType;
            collectionKind = VariadicCollectionKind.Array;
            return true;
        }

        if (type is INamedTypeSymbol named && named.TypeArguments.Length == 1)
        {
            var definition = named.OriginalDefinition;
            var collectionNamespace = definition.ContainingNamespace.ToDisplayString();
            var kind = (collectionNamespace, definition.MetadataName) switch
            {
                ("System.Collections.Generic", "IReadOnlyList`1") =>
                    (VariadicCollectionKind?)VariadicCollectionKind.ReadOnlyList,
                ("System.Collections.Generic", "IReadOnlyCollection`1") =>
                    VariadicCollectionKind.ReadOnlyCollection,
                ("System.Collections.Generic", "IReadOnlySet`1") =>
                    VariadicCollectionKind.ReadOnlySet,
                ("System.Collections.Generic", "IList`1") => VariadicCollectionKind.List,
                ("System.Collections.Generic", "List`1") => VariadicCollectionKind.List,
                ("System.Collections.Generic", "ICollection`1") => VariadicCollectionKind.Collection,
                ("System.Collections.Generic", "IEnumerable`1") => VariadicCollectionKind.Enumerable,
                ("System.Collections.Generic", "HashSet`1") => VariadicCollectionKind.HashSet,
                ("System.Collections.Generic", "ISet`1") => VariadicCollectionKind.Set,
                ("System.Collections.Generic", "LinkedList`1") => VariadicCollectionKind.LinkedList,
                ("System.Collections.Frozen", "FrozenSet`1") => VariadicCollectionKind.FrozenSet,
                ("System.Collections.ObjectModel", "ReadOnlyCollection`1") =>
                    VariadicCollectionKind.ReadOnlyCollectionConcrete,
                ("System.Collections.ObjectModel", "Collection`1") =>
                    VariadicCollectionKind.CollectionConcrete,
                ("System.Collections.Immutable", "ImmutableArray`1") =>
                    VariadicCollectionKind.ImmutableArray,
                ("System.Collections.Immutable", "ImmutableList`1") =>
                    VariadicCollectionKind.ImmutableList,
                ("System.Collections.Immutable", "ImmutableHashSet`1") =>
                    VariadicCollectionKind.ImmutableHashSet,
                _ => null,
            };

            if (kind is not null)
            {
                elementType = named.TypeArguments[0];
                collectionKind = kind.Value;
                return true;
            }
        }

        elementType = null!;
        collectionKind = default;
        return false;
    }

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
        ITypeSymbol? enumParser,
        out ParsePlan parsePlan)
    {
        if (parserType is not null)
        {
            if (!IsValidParameterParser(parserType, valueType))
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
            parsePlan = enumParser is null
                ? new ParsePlan(
                    ParseKind.Enum,
                    valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    null)
                : new ParsePlan(
                    ParseKind.EnumCustom,
                    valueType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    enumParser.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
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

    private static bool IsValidParameterParser(ITypeSymbol parserType, ITypeSymbol valueType)
    {
        if (parserType is not INamedTypeSymbol { IsStatic: false, IsUnboundGenericType: false } parser)
        {
            return false;
        }

        return parser.AllInterfaces.Any(
            currentInterface =>
                currentInterface.OriginalDefinition.MetadataName == ParameterParserInterfaceName &&
                currentInterface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "AnyCli.Parameters.Parsing" &&
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

    private static RegistrationSet GetParameterParserRegistrations(
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        var parsers = new Dictionary<ITypeSymbol, ITypeSymbol>(SymbolEqualityComparer.Default);
        var diagnostics = new List<DiagnosticModel>();
        ITypeSymbol? enumParser = null;

        foreach (var attribute in compilation.Assembly.GetAttributes())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null ||
                !attributeClass.IsGenericType ||
                attributeClass.OriginalDefinition.ContainingNamespace.ToDisplayString() != "AnyCli.Parameters.Parsing")
            {
                continue;
            }

            if (attributeClass.OriginalDefinition.MetadataName == EnumParameterParserAttributeName &&
                attributeClass.TypeArguments.Length == 1)
            {
                enumParser = attributeClass.TypeArguments[0];
                continue;
            }

            if (attributeClass.OriginalDefinition.MetadataName != ParameterValueParserAttributeName ||
                attributeClass.TypeArguments.Length != 2)
            {
                continue;
            }

            var valueType = attributeClass.TypeArguments[0];
            var parserType = attributeClass.TypeArguments[1];
            if (parsers.ContainsKey(valueType))
            {
                diagnostics.Add(new DiagnosticModel(
                    DuplicateParameterParserDiagnostic,
                    GetLocation(attribute, cancellationToken),
                    valueType.ToDisplayString()));
                continue;
            }

            parsers[valueType] = parserType;
        }

        return new RegistrationSet(parsers, enumParser, diagnostics);
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

    private static bool ImplementsParametersInterface(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(
            currentInterface =>
                currentInterface.OriginalDefinition.MetadataName == ParametersInterfaceName &&
                currentInterface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "AnyCli.Parameters.Binding" &&
                currentInterface.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(currentInterface.TypeArguments[0], type));

    private static bool ImplementsGlobalOptionsInterface(INamedTypeSymbol type) =>
        type.AllInterfaces.Any(
            currentInterface =>
                currentInterface.OriginalDefinition.MetadataName == GlobalOptionsInterfaceName &&
                currentInterface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "AnyCli.Parameters.Options" &&
                currentInterface.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(currentInterface.TypeArguments[0], type));

    private static bool HasGlobalOptionsAttribute(INamedTypeSymbol type) =>
        type.GetAttributes().Any(
            attribute => attribute.AttributeClass?.ToDisplayString() == GlobalOptionsAttributeName);

    private static INamedTypeSymbol? GetGlobalOptionsBase(INamedTypeSymbol type)
    {
        for (var current = type.BaseType;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            if (HasGlobalOptionsAttribute(current))
            {
                return current;
            }
        }

        return null;
    }

    private static bool IsDeclaredInTypeOrBase(
        INamedTypeSymbol baseType,
        INamedTypeSymbol containingType)
    {
        for (var current = baseType;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, containingType))
            {
                return true;
            }
        }

        return false;
    }

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

    private static void Emit(SourceProductionContext context, ParametersModel model)
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

        if (model.IsGlobalOptions)
        {
            if (!model.ImplementsGlobalOptionsInterface)
            {
                source.Append(" : global::AnyCli.Parameters.Options.IGlobalOptions<")
                    .Append(model.FullTypeName)
                    .Append('>');
            }
        }
        else if (!model.ImplementsParametersInterface)
        {
            source.Append(" : global::AnyCli.Parameters.Binding.ICommandParameters<").Append(model.FullTypeName).Append('>');
        }

        source.AppendLine();
        source.Append(indent).AppendLine("{");

        if (model.IsGlobalOptions)
        {
            source.Append(indent).AppendLine("    public static void Configure(global::AnyCli.Parameters.Options.GlobalOptionBuilder options)");
            source.Append(indent).AppendLine("    {");
            EmitOptionConfiguration(source, indent, model.Options, "options", 0);
            source.Append(indent).AppendLine("    }");
            source.AppendLine();

            source.Append(indent)
                .Append("    public static ")
                .Append(model.FullTypeName)
                .AppendLine(" Bind(global::AnyCli.Parameters.Options.CommandOptionValues values)");
            source.Append(indent).AppendLine("    {");
            source.Append(indent).Append("        var parameters = new ").Append(model.FullTypeName).AppendLine("();");
            EmitOptionBindings(
                source,
                indent,
                model.Options,
                "values",
                0,
                "global::AnyCli.Errors.ParameterSource.GlobalOption");
            source.Append(indent).AppendLine("        return parameters;");
            source.Append(indent).AppendLine("    }");
            source.Append(indent).AppendLine("}");

            if (hasNamespace)
            {
                source.AppendLine("}");
            }

            context.AddSource(
                model.FullTypeName.Replace("global::", string.Empty).Replace('.', '_') + ".CommandParameters.g.cs",
                SourceText.From(source.ToString(), Encoding.UTF8));
            return;
        }

        source.Append(indent).AppendLine("    public static void Configure(global::AnyCli.Parameters.Binding.CommandParameterDefinitionBuilder parameters)");
        source.Append(indent).AppendLine("    {");
        foreach (var argument in model.Arguments)
        {
            source.Append(indent).Append("        parameters.Arguments.");
            if (argument.Variadic)
            {
                source.Append("AddVariadic<")
                    .Append(argument.ElementTypeFullName)
                    .Append(">(")
                    .Append(SymbolDisplay.FormatLiteral(argument.Name, true))
                    .Append(", typeof(")
                    .Append(argument.CollectionTypeFullName)
                    .Append("), ")
                    .Append(GetVariadicArityExpression(argument.MinimumCount, argument.MaximumCount))
                    .AppendLine(");");
            }
            else
            {
                source.Append(argument.Optional ? "AddOptional" : "Add")
                    .Append('<')
                    .Append(argument.ElementTypeFullName)
                    .Append(">(")
                    .Append(SymbolDisplay.FormatLiteral(argument.Name, true))
                    .AppendLine(");");
            }

            if (argument.Description.Length > 0)
            {
                source.Append(indent)
                    .Append("        parameters.Arguments.Describe(")
                    .Append(SymbolDisplay.FormatLiteral(argument.Description, true))
                    .AppendLine(");");
            }

            if (argument.DefaultValueExpression is not null)
            {
                source.Append(indent)
                    .Append("        parameters.Arguments.")
                    .Append(argument.Variadic ? "SetDefaultValues" : "SetDefaultValue")
                    .Append('(')
                    .Append(argument.DefaultValueExpression)
                    .AppendLine(");");
            }
        }

        EmitOptionConfiguration(source, indent, model.Options, "parameters.Options", 0);

        source.Append(indent).AppendLine("    }");
        source.AppendLine();

        source.Append(indent)
            .Append("    public static ")
            .Append(model.FullTypeName)
            .AppendLine(" Bind(")
            .Append(indent)
            .AppendLine("        global::AnyCli.Parameters.Binding.ParsedCommandParameters parameterValues,")
            .Append(indent)
            .AppendLine("        global::AnyCli.Parameters.Options.CommandOptionValues globalOptionValues)");
        source.Append(indent).AppendLine("    {");
        source.Append(indent).Append("        var parameters = new ").Append(model.FullTypeName).AppendLine("();");

        var localIndex = 0;
        foreach (var argument in model.Arguments)
        {
            var literalName = SymbolDisplay.FormatLiteral(argument.Name, true);
            if (argument.Variadic)
            {
                var rawLocal = "__raw" + localIndex;
                var valuesLocal = "__values" + localIndex;
                var indexLocal = "__i" + localIndex;
                source.Append(indent)
                    .Append("        var ")
                    .Append(rawLocal)
                    .Append(" = parameterValues.Arguments.GetVariadic(")
                    .Append(literalName)
                    .AppendLine(");");

                if (argument.DefaultValueExpression is not null)
                {
                    source.Append(indent)
                        .Append("        if (")
                        .Append(rawLocal)
                        .AppendLine(".Count > 0)");
                    source.Append(indent).AppendLine("        {");
                }

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
                        argument.ParsePlan,
                        rawLocal + "[" + indexLocal + "]",
                        literalName,
                        "global::AnyCli.Errors.ParameterSource.Argument"))
                    .AppendLine(";");
                source.Append(indent).AppendLine("        }");
                source.Append(indent)
                    .Append("        parameters.")
                    .Append(argument.PropertyName)
                    .Append(" = ")
                    .Append(GetCollectionExpression(
                        argument.CollectionKind,
                        argument.ElementTypeFullName,
                        valuesLocal))
                    .AppendLine(";");

                if (argument.DefaultValueExpression is not null)
                {
                    source.Append(indent).AppendLine("        }");
                    source.Append(indent).AppendLine("        else");
                    source.Append(indent).AppendLine("        {");
                    source.Append(indent)
                        .Append("        parameters.")
                        .Append(argument.PropertyName)
                        .Append(" = ")
                        .Append(GetCollectionExpression(
                            argument.CollectionKind,
                            argument.ElementTypeFullName,
                            argument.DefaultValueExpression))
                        .AppendLine(";");
                    source.Append(indent).AppendLine("        }");
                }
            }
            else if (argument.Optional)
            {
                var rawLocal = "__raw" + localIndex;
                source.Append(indent)
                    .Append("        if (parameterValues.Arguments.TryGetOptional(")
                    .Append(literalName)
                    .Append(", out var ")
                    .Append(rawLocal)
                    .AppendLine("))");
                source.Append(indent).AppendLine("        {");
                source.Append(indent)
                    .Append("            parameters.")
                    .Append(argument.PropertyName)
                    .Append(" = ")
                    .Append(GetParseExpression(
                        argument.ParsePlan,
                        rawLocal + "!",
                        literalName,
                        "global::AnyCli.Errors.ParameterSource.Argument"))
                    .AppendLine(";");
                source.Append(indent).AppendLine("        }");

                if (argument.DefaultValueExpression is not null)
                {
                    source.Append(indent).AppendLine("        else");
                    source.Append(indent).AppendLine("        {");
                    source.Append(indent)
                        .Append("            parameters.")
                        .Append(argument.PropertyName)
                        .Append(" = ")
                        .Append(argument.DefaultValueExpression)
                        .AppendLine(";");
                    source.Append(indent).AppendLine("        }");
                }
            }
            else
            {
                source.Append(indent)
                    .Append("        parameters.")
                    .Append(argument.PropertyName)
                    .Append(" = ")
                    .Append(GetParseExpression(
                        argument.ParsePlan,
                        "parameterValues.Arguments.GetRequired(" + literalName + ")",
                        literalName,
                        "global::AnyCli.Errors.ParameterSource.Argument"))
                    .AppendLine(";");
            }

            localIndex++;
        }

        EmitOptionBindings(
            source,
            indent,
            model.GlobalOptions,
            "globalOptionValues",
            localIndex,
            "global::AnyCli.Errors.ParameterSource.GlobalOption");
        localIndex += model.GlobalOptions.Count;
        EmitOptionBindings(
            source,
            indent,
            model.Options,
            "parameterValues.Options",
            localIndex,
            "global::AnyCli.Errors.ParameterSource.Option");

        source.Append(indent).AppendLine("        return parameters;");
        source.Append(indent).AppendLine("    }");
        source.Append(indent).AppendLine("}");

        if (hasNamespace)
        {
            source.AppendLine("}");
        }

        context.AddSource(
            model.FullTypeName.Replace("global::", string.Empty).Replace('.', '_') + ".CommandParameters.g.cs",
            SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static void EmitOptionConfiguration(
        StringBuilder source,
        string indent,
        IReadOnlyList<OptionModel> options,
        string optionsExpression,
        int optionIndex)
    {
        foreach (var option in options)
        {
            var local = "__option" + optionIndex++;

            source.Append(indent)
                .Append("        var ")
                .Append(local)
                .Append(" = ")
                .Append(optionsExpression)
                .Append('.');

            if (option.IsFlag)
            {
                source.Append("AddFlag(")
                    .Append(SymbolDisplay.FormatLiteral(option.Name, true))
                    .Append(')');
            }
            else if (option.IsCollection)
            {
                source.Append("AddCollection<")
                    .Append(option.ElementTypeFullName)
                    .Append(">(")
                    .Append(SymbolDisplay.FormatLiteral(option.Name, true))
                    .Append(')');
            }
            else
            {
                source.Append("AddValue<")
                    .Append(option.ElementTypeFullName)
                    .Append(">(")
                    .Append(SymbolDisplay.FormatLiteral(option.Name, true))
                    .Append(')');
            }

            source.AppendLine(";");

            if (option.Aliases.Count > 0)
            {
                source.Append(indent)
                    .Append("        ")
                    .Append(local)
                    .Append(".SetAliases(")
                    .Append(GetStringArrayExpression(option.Aliases))
                    .AppendLine(");");
            }

            if (option.Description.Length > 0)
            {
                source.Append(indent)
                    .Append("        ")
                    .Append(local)
                    .Append(".SetDescription(")
                    .Append(SymbolDisplay.FormatLiteral(option.Description, true))
                    .AppendLine(");");
            }

            if (option.DefaultValueExpression is not null)
            {
                source.Append(indent)
                    .Append("        ")
                    .Append(local)
                    .Append(".SetDefaultValue(")
                    .Append(option.DefaultValueExpression)
                    .AppendLine(");");
            }

            if (option.Required)
            {
                source.Append(indent)
                    .Append("        ")
                    .Append(local)
                    .AppendLine(".SetRequired();");
            }
        }
    }

    private static void EmitOptionBindings(
        StringBuilder source,
        string indent,
        IReadOnlyList<OptionModel> options,
        string valuesExpression,
        int localIndex,
        string parameterSource)
    {
        foreach (var option in options)
        {
            var literalName = SymbolDisplay.FormatLiteral(option.Name, true);
            var rawLocal = "__raw" + localIndex;

            if (option.IsCollection)
            {
                var valuesLocal = "__values" + localIndex;
                var indexLocal = "__i" + localIndex;

                if (option.DefaultValueExpression is not null)
                {
                    source.Append(indent)
                        .Append("        if (")
                        .Append(valuesExpression)
                        .Append(".Has(")
                        .Append(literalName)
                        .AppendLine("))");
                    source.Append(indent).AppendLine("        {");
                }

                source.Append(indent)
                    .Append("        var ")
                    .Append(rawLocal)
                    .Append(" = ")
                    .Append(valuesExpression)
                    .Append(".GetValues(")
                    .Append(literalName)
                    .AppendLine(");");
                source.Append(indent)
                    .Append("        var ")
                    .Append(valuesLocal)
                    .Append(" = new ")
                    .Append(option.ElementTypeFullName)
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
                        option.ParsePlan,
                        rawLocal + "[" + indexLocal + "]",
                        literalName,
                        parameterSource))
                    .AppendLine(";");
                source.Append(indent).AppendLine("        }");
                source.Append(indent)
                    .Append("        parameters.")
                    .Append(option.PropertyName)
                    .Append(" = ")
                    .Append(GetCollectionExpression(
                        option.CollectionKind,
                        option.ElementTypeFullName,
                        valuesLocal))
                    .AppendLine(";");

                if (option.DefaultValueExpression is not null)
                {
                    source.Append(indent).AppendLine("        }");
                    source.Append(indent).AppendLine("        else");
                    source.Append(indent).AppendLine("        {");
                    source.Append(indent)
                        .Append("        parameters.")
                        .Append(option.PropertyName)
                        .Append(" = ")
                        .Append(GetCollectionExpression(
                            option.CollectionKind,
                            option.ElementTypeFullName,
                            option.DefaultValueExpression))
                        .AppendLine(";");
                    source.Append(indent).AppendLine("        }");
                }
            }
            else
            {
                source.Append(indent)
                    .Append("        if (")
                    .Append(valuesExpression)
                    .Append(".TryGetValue(")
                    .Append(literalName)
                    .Append(", out var ")
                    .Append(rawLocal)
                    .AppendLine("))");
                source.Append(indent).AppendLine("        {");
                source.Append(indent)
                    .Append("            parameters.")
                    .Append(option.PropertyName)
                    .Append(" = ");

                if (option.IsFlag)
                {
                    source.Append(rawLocal)
                        .AppendLine(" is null");
                    source.Append(indent).AppendLine("                ? true");
                    source.Append(indent).Append("                : ")
                        .Append(GetParseExpression(
                            option.ParsePlan,
                            rawLocal,
                            literalName,
                            parameterSource))
                        .AppendLine(";");
                }
                else
                {
                    source.Append(GetParseExpression(
                        option.ParsePlan,
                        rawLocal + "!",
                        literalName,
                        parameterSource))
                        .AppendLine(";");
                }

                source.Append(indent).AppendLine("        }");

                if (option.DefaultValueExpression is not null)
                {
                    source.Append(indent).AppendLine("        else");
                    source.Append(indent).AppendLine("        {");
                    source.Append(indent)
                        .Append("            parameters.")
                        .Append(option.PropertyName)
                        .Append(" = ")
                        .Append(option.DefaultValueExpression)
                        .AppendLine(";");
                    source.Append(indent).AppendLine("        }");
                }
            }

            localIndex++;
        }
    }

    private static string GetStringArrayExpression(IReadOnlyList<string> values) =>
        values.Count == 0
            ? "global::System.Array.Empty<string>()"
            : "new string[] { " +
              string.Join(", ", values.Select(value => SymbolDisplay.FormatLiteral(value, true))) +
              " }";

    private static string GetVariadicArityExpression(int minimumCount, int maximumCount)
    {
        if (maximumCount == int.MaxValue)
        {
            return minimumCount switch
            {
                0 => "global::AnyCli.Parameters.Arguments.ArgumentArity.ZeroOrMore",
                1 => "global::AnyCli.Parameters.Arguments.ArgumentArity.OneOrMore",
                _ => $"new global::AnyCli.Parameters.Arguments.ArgumentArity({minimumCount}, global::System.Int32.MaxValue)",
            };
        }

        return $"new global::AnyCli.Parameters.Arguments.ArgumentArity({minimumCount}, {maximumCount})";
    }

    private static string GetCollectionExpression(
        VariadicCollectionKind collectionKind,
        string elementTypeFullName,
        string valuesExpression) =>
        collectionKind switch
        {
            VariadicCollectionKind.Array => valuesExpression,
            VariadicCollectionKind.HashSet or
            VariadicCollectionKind.Set or
            VariadicCollectionKind.ReadOnlySet =>
                $"new global::System.Collections.Generic.HashSet<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.FrozenSet =>
                $"global::System.Collections.Frozen.FrozenSet.ToFrozenSet<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.LinkedList =>
                $"new global::System.Collections.Generic.LinkedList<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.ReadOnlyCollectionConcrete =>
                $"new global::System.Collections.ObjectModel.ReadOnlyCollection<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.CollectionConcrete =>
                $"new global::System.Collections.ObjectModel.Collection<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.ImmutableArray =>
                $"global::System.Collections.Immutable.ImmutableArray.CreateRange<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.ImmutableList =>
                $"global::System.Collections.Immutable.ImmutableList.CreateRange<{elementTypeFullName}>({valuesExpression})",
            VariadicCollectionKind.ImmutableHashSet =>
                $"global::System.Collections.Immutable.ImmutableHashSet.CreateRange<{elementTypeFullName}>({valuesExpression})",
            _ =>
                $"new global::System.Collections.Generic.List<{elementTypeFullName}>({valuesExpression})",
        };

    private static string GetParseExpression(
        ParsePlan parsePlan,
        string rawExpression,
        string literalName,
        string parameterSource) =>
        parsePlan.Kind switch
        {
            ParseKind.String => rawExpression,
            ParseKind.Enum =>
                $"global::AnyCli.Parameters.Parsing.ParameterValueParsing.ParseEnum<{parsePlan.ValueTypeFullName}>(" +
                $"{rawExpression}, {literalName}, {parameterSource})",
            ParseKind.EnumCustom =>
                $"global::AnyCli.Parameters.Parsing.ParameterValueParsing.ParseEnumCustom<{parsePlan.ValueTypeFullName}, " +
                $"{parsePlan.ParserTypeFullName}>({rawExpression}, {literalName}, {parameterSource})",
            ParseKind.SpanParsable =>
                $"global::AnyCli.Parameters.Parsing.ParameterValueParsing.ParseSpanParsable<{parsePlan.ValueTypeFullName}>(" +
                $"{rawExpression}, global::System.Globalization.CultureInfo.InvariantCulture, {literalName}, {parameterSource})",
            ParseKind.Parsable =>
                $"global::AnyCli.Parameters.Parsing.ParameterValueParsing.ParseParsable<{parsePlan.ValueTypeFullName}>(" +
                $"{rawExpression}, global::System.Globalization.CultureInfo.InvariantCulture, {literalName}, {parameterSource})",
            ParseKind.Custom =>
                $"global::AnyCli.Parameters.Parsing.ParameterValueParsing.ParseCustom<{parsePlan.ValueTypeFullName}, " +
                $"{parsePlan.ParserTypeFullName}>({rawExpression}, " +
                $"global::System.Globalization.CultureInfo.InvariantCulture, {literalName}, {parameterSource})",
            _ => throw new InvalidOperationException("Unknown parse kind."),
        };

    private sealed class OptionModel(
        string name,
        IReadOnlyList<string> aliases,
        string description,
        string propertyName,
        bool required,
        bool isFlag,
        bool isCollection,
        string elementTypeFullName,
        VariadicCollectionKind collectionKind,
        string? defaultValueExpression,
        ParsePlan parsePlan)
    {
        public string Name { get; } = name;

        public IReadOnlyList<string> Aliases { get; } = aliases;

        public IReadOnlyList<string> Names { get; } = [name, .. aliases];

        public string Description { get; } = description;

        public string PropertyName { get; } = propertyName;

        public bool Required { get; } = required;

        public bool IsFlag { get; } = isFlag;

        public bool IsCollection { get; } = isCollection;

        public string ElementTypeFullName { get; } = elementTypeFullName;

        public VariadicCollectionKind CollectionKind { get; } = collectionKind;

        public string? DefaultValueExpression { get; } = defaultValueExpression;

        public ParsePlan ParsePlan { get; } = parsePlan;
    }

    private sealed class ParametersModel
    {
        public ParametersModel(
            string @namespace,
            string typeName,
            string fullTypeName,
            string accessibility,
            bool isSealed,
            bool isRecord,
            bool isGlobalOptions,
            bool implementsGlobalOptionsInterface,
            bool implementsParametersInterface,
            IReadOnlyList<ArgumentModel> arguments,
            IReadOnlyList<OptionModel> options,
            IReadOnlyList<OptionModel> globalOptions,
            IReadOnlyList<DiagnosticModel> diagnostics)
        {
            Namespace = @namespace;
            TypeName = typeName;
            FullTypeName = fullTypeName;
            Accessibility = accessibility;
            IsSealed = isSealed;
            IsRecord = isRecord;
            IsGlobalOptions = isGlobalOptions;
            ImplementsGlobalOptionsInterface = implementsGlobalOptionsInterface;
            ImplementsParametersInterface = implementsParametersInterface;
            Arguments = arguments;
            Options = options;
            GlobalOptions = globalOptions;
            Diagnostics = diagnostics;
        }

        public string Namespace { get; }

        public string TypeName { get; }

        public string FullTypeName { get; }

        public string Accessibility { get; }

        public bool IsSealed { get; }

        public bool IsRecord { get; }

        public bool IsGlobalOptions { get; }

        public bool ImplementsGlobalOptionsInterface { get; }

        public bool ImplementsParametersInterface { get; }

        public IReadOnlyList<ArgumentModel> Arguments { get; }

        public IReadOnlyList<OptionModel> Options { get; }

        public IReadOnlyList<OptionModel> GlobalOptions { get; }

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
            string collectionTypeFullName,
            VariadicCollectionKind collectionKind,
            int minimumCount,
            int maximumCount,
            string? defaultValueExpression,
            ParsePlan parsePlan)
        {
            Name = name;
            Description = description;
            PropertyName = propertyName;
            Position = position;
            Optional = optional;
            Variadic = variadic;
            ElementTypeFullName = elementTypeFullName;
            CollectionTypeFullName = collectionTypeFullName;
            CollectionKind = collectionKind;
            MinimumCount = minimumCount;
            MaximumCount = maximumCount;
            DefaultValueExpression = defaultValueExpression;
            ParsePlan = parsePlan;
        }

        public string Name { get; }

        public string Description { get; }

        public string PropertyName { get; }

        public int Position { get; }

        public bool Optional { get; }

        public bool Variadic { get; }

        public string ElementTypeFullName { get; }

        public string CollectionTypeFullName { get; }

        public VariadicCollectionKind CollectionKind { get; }

        public int MinimumCount { get; }

        public int MaximumCount { get; }

        public string? DefaultValueExpression { get; }

        public ParsePlan ParsePlan { get; }
    }

    private sealed class RegistrationSet(
        IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> parsers,
        ITypeSymbol? enumParser,
        IReadOnlyList<DiagnosticModel> diagnostics)
    {
        public IReadOnlyDictionary<ITypeSymbol, ITypeSymbol> Parsers { get; } = parsers;

        public ITypeSymbol? EnumParser { get; } = enumParser;

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

    private enum VariadicCollectionKind
    {
        Array,
        ReadOnlyList,
        ReadOnlyCollection,
        ReadOnlySet,
        List,
        Collection,
        Enumerable,
        HashSet,
        Set,
        FrozenSet,
        LinkedList,
        ReadOnlyCollectionConcrete,
        CollectionConcrete,
        ImmutableArray,
        ImmutableList,
        ImmutableHashSet,
    }

    private enum ParseKind
    {
        String,
        Enum,
        EnumCustom,
        SpanParsable,
        Parsable,
        Custom,
    }
}
