// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Resolves parameter parser instances.
/// </summary>
/// <remarks>
/// A registered parser is resolved from the configured service provider. Otherwise, a parser with
/// a public parameterless constructor is created directly.
/// </remarks>
internal static class ParameterParserResolver
{
    /// <summary>
    /// Resolves a parameter parser.
    /// </summary>
    /// <typeparam name="TParser">The parameter parser type.</typeparam>
    /// <param name="services">The configured service provider, or <see langword="null"/>.</param>
    /// <returns>The resolved parser.</returns>
    /// <exception cref="InvalidOperationException">
    /// The parser could not be created.
    /// </exception>
    public static TParser Resolve<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TParser>(
        IServiceProvider? services)
        where TParser : class
    {
        return services is not null && services.GetService(typeof(TParser)) is TParser registeredParser
            ? registeredParser
            : typeof(TParser).GetConstructor(Type.EmptyTypes) is not null
                ? Activator.CreateInstance<TParser>() ?? throw new InvalidOperationException($"Could not create parameter parser '{typeof(TParser).FullName}'.")
                : throw new InvalidOperationException(
                    $"Parameter parser '{typeof(TParser).FullName}' is not registered in the service provider and has no public parameterless constructor.");
    }
}
