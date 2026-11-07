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

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsCandidate(node),
                static (syntaxContext, cancellationToken) => GetModel(syntaxContext, cancellationToken))
            .Where(static model => model is not null);

        context.RegisterSourceOutput(
            models,
            static (productionContext, model) => Emit(productionContext, model!));
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

        var arguments = new List<ArgumentModel>();
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
                }

                if (isVariadic)
                {
                    if (property.Type is not IArrayTypeSymbol array ||
                        array.Rank != 1 ||
                        array.ElementType.SpecialType != SpecialType.System_String)
                    {
                        return null;
                    }

                    optional = false;
                }
                else if (property.Type.SpecialType != SpecialType.System_String)
                {
                    return null;
                }

                arguments.Add(new ArgumentModel(name, description, property.Name, position, optional, isVariadic));
            }
        }

        if (arguments.Count == 0)
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
            arguments.OrderBy(argument => argument.Position).ToArray());
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

            source.Append('(').Append(SymbolDisplay.FormatLiteral(argument.Name, true)).AppendLine(");");

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

        foreach (var argument in model.Arguments)
        {
            var literalName = SymbolDisplay.FormatLiteral(argument.Name, true);
            if (argument.Variadic)
            {
                source.Append(indent)
                    .Append("        settings.")
                    .Append(argument.PropertyName)
                    .Append(" = global::System.Linq.Enumerable.ToArray(arguments.GetVariadic(")
                    .Append(literalName)
                    .AppendLine("));");
            }
            else if (argument.Optional)
            {
                var localName = "__" + argument.PropertyName;
                source.Append(indent)
                    .Append("        if (arguments.TryGetOptional(")
                    .Append(literalName)
                    .Append(", out var ")
                    .Append(localName)
                    .AppendLine("))");
                source.Append(indent).AppendLine("        {");
                source.Append(indent)
                    .Append("            settings.")
                    .Append(argument.PropertyName)
                    .Append(" = ")
                    .Append(localName)
                    .AppendLine("!;");
                source.Append(indent).AppendLine("        }");
            }
            else
            {
                source.Append(indent)
                    .Append("        settings.")
                    .Append(argument.PropertyName)
                    .Append(" = arguments.GetRequired(")
                    .Append(literalName)
                    .AppendLine(");");
            }
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
            IReadOnlyList<ArgumentModel> arguments)
        {
            Namespace = @namespace;
            TypeName = typeName;
            FullTypeName = fullTypeName;
            Accessibility = accessibility;
            IsSealed = isSealed;
            IsRecord = isRecord;
            ImplementsSettingsInterface = implementsSettingsInterface;
            Arguments = arguments;
        }

        public string Namespace { get; }

        public string TypeName { get; }

        public string FullTypeName { get; }

        public string Accessibility { get; }

        public bool IsSealed { get; }

        public bool IsRecord { get; }

        public bool ImplementsSettingsInterface { get; }

        public IReadOnlyList<ArgumentModel> Arguments { get; }
    }

    private sealed class ArgumentModel
    {
        public ArgumentModel(
            string name,
            string description,
            string propertyName,
            int position,
            bool optional,
            bool variadic)
        {
            Name = name;
            Description = description;
            PropertyName = propertyName;
            Position = position;
            Optional = optional;
            Variadic = variadic;
        }

        public string Name { get; }

        public string Description { get; }

        public string PropertyName { get; }

        public int Position { get; }

        public bool Optional { get; }

        public bool Variadic { get; }
    }
}
