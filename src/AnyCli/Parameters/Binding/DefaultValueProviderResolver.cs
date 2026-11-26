// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli.Parameters.Binding;

/// <summary>
/// Resolves default value providers from the configured service provider.
/// </summary>
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
        return services is null
            ? Activator.CreateInstance<TProvider>()
                ?? throw new InvalidOperationException(
                    $"Could not create default value provider '{typeof(TProvider).FullName}'.")
            : services.GetService(typeof(TProvider)) as TProvider
                ?? throw new InvalidOperationException(
                    $"Service '{typeof(TProvider).FullName}' is not registered.");
    }
}
