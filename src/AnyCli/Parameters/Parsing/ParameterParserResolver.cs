// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Resolves asynchronous parameter parsers from the configured service provider.
/// </summary>
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
        return services is null
            ? Activator.CreateInstance<TParser>()
                ?? throw new InvalidOperationException(
                    $"Could not create parameter parser '{typeof(TParser).FullName}'.")
            : services.GetService(typeof(TParser)) as TParser
                ?? throw new InvalidOperationException(
                    $"Service '{typeof(TParser).FullName}' is not registered.");
    }
}
