// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

namespace AnyCli;

public sealed class ConsoleAppBuilder
{
    private readonly Dictionary<string, Func<IServiceProvider?, CommandContext, Task<int>>> _commands = new(StringComparer.Ordinal);

    private readonly Dictionary<string, CommandInfo> _commandInfos = new(StringComparer.Ordinal);
    private IHelpProvider? _helpProvider;
    private IServiceProvider? _serviceProvider;
    private string? _name;
    private string? _version;

    private ConsoleAppBuilder()
    {
    }

    public static ConsoleAppBuilder Create() => new();

    public ConsoleAppBuilder WithName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _name = name;
        return this;
    }

    public ConsoleAppBuilder WithVersion(string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        _version = version;
        return this;
    }

    public ConsoleAppBuilder UseServiceProvider(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
        return this;
    }

    public ConsoleAppBuilder AddCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : class, ICommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _commands[name] = (serviceProvider, context) =>
            Task.FromResult(ResolveCommand<TCommand>(serviceProvider).Execute(context));
        _commandInfos[name] = new CommandInfo(name, description);
        return this;
    }

    public ConsoleAppBuilder AddAsyncCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        string name,
        string description = "")
        where TCommand : class, IAsyncCommand
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _commands[name] = (serviceProvider, context) => ResolveCommand<TCommand>(serviceProvider).ExecuteAsync(context);
        _commandInfos[name] = new CommandInfo(name, description);
        return this;
    }

    public ConsoleAppBuilder AddHelp<THelpProvider>()
        where THelpProvider : class, IHelpProvider, new()
    {
        _helpProvider = new THelpProvider();
        return this;
    }

    public ConsoleApp Build() =>
        new(
            _name,
            _version,
            new Dictionary<string, Func<IServiceProvider?, CommandContext, Task<int>>>(_commands, StringComparer.Ordinal),
            [.. _commandInfos.Values],
            _serviceProvider,
            _helpProvider);

    private static TCommand ResolveCommand<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TCommand>(
        IServiceProvider? serviceProvider)
        where TCommand : class
    {
        return serviceProvider is null
            ? Activator.CreateInstance<TCommand>()
                ?? throw new InvalidOperationException($"Could not create command '{typeof(TCommand).FullName}'.")
            : serviceProvider.GetService(typeof(TCommand)) as TCommand
                ?? throw new InvalidOperationException($"Service '{typeof(TCommand).FullName}' is not registered.");
    }
}
