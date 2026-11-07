// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

public sealed class ConsoleApp
{
    private readonly Dictionary<string, Func<IServiceProvider?, CommandContext, Task<int>>> _commands;
    private readonly IReadOnlyList<CommandInfo> _commandInfos;
    private readonly IHelpProvider? _helpProvider;
    private readonly string? _name;
    private readonly IServiceProvider? _serviceProvider;
    private readonly string? _version;

    internal ConsoleApp(
        string? name,
        string? version,
        Dictionary<string, Func<IServiceProvider?, CommandContext, Task<int>>> commands,
        IReadOnlyList<CommandInfo> commandInfos,
        IServiceProvider? serviceProvider,
        IHelpProvider? helpProvider)
    {
        _name = name;
        _version = version;
        _commands = commands;
        _commandInfos = commandInfos;
        _serviceProvider = serviceProvider;
        _helpProvider = helpProvider;
    }

    public Task<int> RunAsync(string[] args) => RunAsync(args, Console.Out, Console.Error);

    public async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (IsVersionRequest(args))
        {
            output.WriteLine(_version);
            return 0;
        }

        if (_helpProvider is not null && IsHelpRequest(args))
        {
            var commandName = args.Length == 2 ? args[0] : null;
            _helpProvider.WriteHelp(new HelpContext(_name ?? string.Empty, output, commandName, _commandInfos, GetOptions()));
            return 0;
        }

        return args.Length == 1 && _commands.TryGetValue(args[0], out var execute)
            ? await execute(_serviceProvider, new CommandContext(args[0], output, error)).ConfigureAwait(false)
            : throw new NotImplementedException();
    }

    private static bool IsVersionRequest(string[] args) =>
        args.Length == 1 && args[0] is "--version" or "-v";

    private bool IsHelpRequest(string[] args) =>
        (args.Length == 1 && IsHelpFlag(args[0])) ||
        (args.Length == 2 && _commands.ContainsKey(args[0]) && IsHelpFlag(args[1]));

    private static bool IsHelpFlag(string arg) => arg is "-h" or "--help";

    private IReadOnlyList<OptionInfo> GetOptions()
    {
        var options = new List<OptionInfo>(2);

        if (_helpProvider is not null) options.Add(new OptionInfo(["-h", "--help"], "Prints help information"));
        if (_version is not null) options.Add(new OptionInfo(["-v", "--version"], "Prints version information"));

        return options;
    }
}
