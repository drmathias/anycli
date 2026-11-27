// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Parameters.Binding;

/// <summary>
/// Resolves default value provider instances.
/// </summary>
/// <remarks>
/// A registered provider is resolved from the configured service provider. Otherwise, a provider
/// with a public parameterless constructor is created directly.
/// </remarks>
public static class DefaultValueProviderResolver
{
    /// <summary>
    /// Resolves a default value provider.
    /// </summary>
    /// <typeparam name="TProvider">The default value provider type.</typeparam>
    /// <param name="services">The configured service provider, or <see langword="null"/>.</param>
    /// <returns>The resolved provider.</returns>
    /// <exception cref="InvalidOperationException">
    /// The provider could not be created.
    /// </exception>
    public static TProvider Resolve<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TProvider>(
        IServiceProvider? services)
        where TProvider : class
    {
        if (services is not null &&
            services.GetService(typeof(TProvider)) is TProvider registeredProvider)
        {
            return registeredProvider;
        }

        if (typeof(TProvider).GetConstructor(Type.EmptyTypes) is not null)
        {
            return Activator.CreateInstance<TProvider>()
                ?? throw new InvalidOperationException(
                    $"Could not create default value provider '{typeof(TProvider).FullName}'.");
        }

        throw new InvalidOperationException(
            $"Default value provider '{typeof(TProvider).FullName}' is not registered in the service provider and has no public parameterless constructor.");
    }
}
