// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// A configured command-line application.
/// </summary>
/// <remarks>
/// Instances are created with <see cref="ConsoleAppBuilder.Build"/>.
/// </remarks>
public sealed class ConsoleApp
{
    private readonly Dictionary<string, CommandRegistration> _commands;
    private readonly IReadOnlyList<CommandInfo> _commandInfos;
    private readonly Func<IServiceProvider?, IHelpProvider>? _helpProviderFactory;
    private readonly string? _name;
    private readonly IServiceProvider? _serviceProvider;
    private readonly Func<IServiceProvider?, IUnrecognizedCommandHandler> _unrecognizedCommandHandlerFactory;
    private readonly int _unrecognizedCommandExitCode;
    private readonly string? _version;

    internal ConsoleApp(
        string? name,
        string? version,
        Dictionary<string, CommandRegistration> commands,
        IReadOnlyList<CommandInfo> commandInfos,
        IServiceProvider? serviceProvider,
        Func<IServiceProvider?, IHelpProvider>? helpProviderFactory,
        Func<IServiceProvider?, IUnrecognizedCommandHandler> unrecognizedCommandHandlerFactory,
        int unrecognizedCommandExitCode)
    {
        _name = name;
        _version = version;
        _commands = commands;
        _commandInfos = commandInfos;
        _serviceProvider = serviceProvider;
        _helpProviderFactory = helpProviderFactory;
        _unrecognizedCommandHandlerFactory = unrecognizedCommandHandlerFactory;
        _unrecognizedCommandExitCode = unrecognizedCommandExitCode;
    }

    /// <summary>
    /// Runs the application using the system console.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The process exit code.</returns>
    public Task<int> RunAsync(string[] args) =>
        RunAsync(args, new TextConsole(Console.In, Console.Out, Console.Error));

    /// <summary>
    /// Runs the application using the specified writers and the system input.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="output">The writer for standard output.</param>
    /// <param name="error">The writer for error output.</param>
    /// <returns>The process exit code.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="output"/> or <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        return RunAsync(args, new TextConsole(Console.In, output, error));
    }

    /// <summary>
    /// Runs the application using the specified console.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="console">The console used for input, output, and errors.</param>
    /// <returns>The process exit code.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="args"/> or <paramref name="console"/> is <see langword="null"/>.
    /// </exception>
    public async Task<int> RunAsync(string[] args, IConsole console)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(console);

        var outputWriter = console.Output;

        if (IsVersionRequest(args))
        {
            outputWriter.WriteLine(_version);
            return 0;
        }

        if (_helpProviderFactory is not null && IsHelpRequest(args))
        {
            var commandName = args.Length == 2 ? args[0] : null;
            var commandInfo = commandName is null
                ? null
                : _commandInfos.FirstOrDefault(info => info.Name == commandName);

            _helpProviderFactory(_serviceProvider).WriteHelp(
                new HelpContext(
                    _name ?? string.Empty,
                    console,
                    commandName,
                    commandInfo?.Arguments ?? [],
                    _commandInfos,
                    GetOptions()));
            return 0;
        }

        if (args.Length >= 1 && _commands.TryGetValue(args[0], out var command))
        {
            var arguments = CommandArgumentsParser.Parse(
                command.Arguments,
                args.AsSpan(1));
            return await command.Execute
                .Invoke(_serviceProvider, new CommandContext(args[0], console), arguments)
                .ConfigureAwait(false);
        }

        var context = new UnrecognizedCommandContext(
            _name,
            args.Length >= 1 ? args[0] : string.Empty,
            [.. _commandInfos.Select(info => info.Name)],
            console);

        _unrecognizedCommandHandlerFactory(_serviceProvider).Handle(context);
        return _unrecognizedCommandExitCode;
    }

    private static bool IsVersionRequest(string[] args) =>
        args.Length == 1 && args[0] is "--version" or "-v";

    private bool IsHelpRequest(string[] args) =>
        (args.Length == 1 && IsHelpFlag(args[0])) ||
        (args.Length == 2 && _commands.ContainsKey(args[0]) && IsHelpFlag(args[1]));

    private static bool IsHelpFlag(string arg) => arg is "-h" or "--help";

    private List<OptionInfo> GetOptions()
    {
        var options = new List<OptionInfo>(2);

        if (_helpProviderFactory is not null) options.Add(new OptionInfo(["-h", "--help"], "Prints help information"));
        if (_version is not null) options.Add(new OptionInfo(["-v", "--version"], "Prints version information"));

        return options;
    }
}
