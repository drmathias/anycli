// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests.Helpers;

internal sealed class StubServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, Func<object>> _factories = [];

    public void Add<TService>(Func<TService> factory) where TService : class => _factories[typeof(TService)] = factory;

    public object? GetService(Type serviceType) => _factories.TryGetValue(serviceType, out var factory) ? factory() : null;
}
