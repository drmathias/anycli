namespace AnyCli;

public sealed class ConsoleAppBuilder
{
    private readonly Dictionary<string, Func<CommandContext, Task<int>>> _commands = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CommandInfo> _commandInfos = new(StringComparer.Ordinal);
    private IHelpProvider? _helpProvider;
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

    public ConsoleAppBuilder AddCommand<TCommand>(string name, string description = "")
        where TCommand : class, ICommand, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _commands[name] = context => Task.FromResult(new TCommand().Execute(context));
        _commandInfos[name] = new CommandInfo(name, description);
        return this;
    }

    public ConsoleAppBuilder AddAsyncCommand<TCommand>(string name, string description = "")
        where TCommand : class, IAsyncCommand, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _commands[name] = context => new TCommand().ExecuteAsync(context);
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
            new Dictionary<string, Func<CommandContext, Task<int>>>(_commands, StringComparer.Ordinal),
            [.. _commandInfos.Values],
            _helpProvider);
}
