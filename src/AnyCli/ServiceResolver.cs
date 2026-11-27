// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli;

/// <summary>
/// Resolves framework service instances from DI or their public parameterless constructor.
/// </summary>
internal static class ServiceResolver
{
    /// <summary>
    /// Resolves a service instance.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <param name="services">The configured service provider, or <see langword="null"/>.</param>
    /// <param name="serviceDescription">A description used in error messages.</param>
    /// <returns>The resolved service.</returns>
    /// <exception cref="InvalidOperationException">
    /// The service could not be created.
    /// </exception>
    public static T Resolve<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(
        IServiceProvider? services,
        string serviceDescription)
        where T : class
    {
        return services is not null && services.GetService(typeof(T)) is T registeredService
            ? registeredService
            : typeof(T).GetConstructor(Type.EmptyTypes) is not null
                ? Activator.CreateInstance<T>()
                    ?? throw new InvalidOperationException(
                        $"Could not create {serviceDescription} '{typeof(T).FullName}'.")
                : throw new InvalidOperationException(
                    $"{serviceDescription} '{typeof(T).FullName}' is not registered in the service provider and has no public parameterless constructor.");
    }
}
