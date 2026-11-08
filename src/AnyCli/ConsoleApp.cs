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
    private readonly IReadOnlyList<OptionDefinition> _globalOptions;
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
        IReadOnlyList<OptionDefinition> globalOptions,
        IServiceProvider? serviceProvider,
        Func<IServiceProvider?, IHelpProvider>? helpProviderFactory,
        Func<IServiceProvider?, IUnrecognizedCommandHandler> unrecognizedCommandHandlerFactory,
        int unrecognizedCommandExitCode)
    {
        _name = name;
        _version = version;
        _commands = commands;
        _commandInfos = commandInfos;
        _globalOptions = globalOptions;
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
        var globalOptionValues = new Dictionary<string, OptionValue>(StringComparer.Ordinal);
        var commandIndex = CommandArgumentsParser.ParseLeadingGlobalOptions(
            _globalOptions,
            args,
            globalOptionValues,
            _helpProviderFactory is not null,
            _version is not null,
            out var leadingHelpRequested,
            out var leadingVersionRequested);

        if (leadingVersionRequested)
        {
            outputWriter.WriteLine(_version);
            return 0;
        }

        if (leadingHelpRequested)
        {
            WriteRootHelp(console);
            return 0;
        }

        var commandName = commandIndex >= 0 && commandIndex < args.Length
            ? args[commandIndex]
            : string.Empty;

        if (commandIndex >= 0 && _commands.TryGetValue(args[commandIndex], out var command))
        {
            var arguments = CommandArgumentsParser.Parse(
                command.Arguments,
                command.Options,
                _globalOptions,
                args.AsSpan(commandIndex + 1),
                globalOptionValues,
                _helpProviderFactory is not null,
                _version is not null,
                out var helpRequested,
                out var versionRequested);

            if (versionRequested)
            {
                outputWriter.WriteLine(_version);
                return 0;
            }

            if (helpRequested)
            {
                WriteCommandHelp(console, commandName);
                return 0;
            }

            ValidateRequiredGlobalOptions(globalOptionValues);

            return await command.Execute
                .Invoke(
                    _serviceProvider,
                    new CommandContext(
                        commandName,
                        console,
                        new CommandOptionValues(globalOptionValues)),
                    arguments!)
                .ConfigureAwait(false);
        }

        var context = new UnrecognizedCommandContext(
            _name,
            commandName,
            [.. _commandInfos.Select(info => info.Name)],
            console);

        _unrecognizedCommandHandlerFactory(_serviceProvider).Handle(context);
        return _unrecognizedCommandExitCode;
    }

    private void WriteRootHelp(IConsole console)
    {
        _helpProviderFactory!(_serviceProvider).WriteRootHelp(
            new RootHelpContext(
                _name ?? string.Empty,
                console,
                GetOptions(),
                _commandInfos));
    }

    private void WriteCommandHelp(IConsole console, string commandName)
    {
        var command = _commandInfos.First(info => info.Name == commandName);
        _helpProviderFactory!(_serviceProvider).WriteCommandHelp(
            new CommandHelpContext(
                _name ?? string.Empty,
                console,
                GetOptions(),
                command));
    }

    private void ValidateRequiredGlobalOptions(
        IReadOnlyDictionary<string, OptionValue> globalOptionValues)
    {
        foreach (var option in _globalOptions)
        {
            if (option.Required && !globalOptionValues.ContainsKey(option.Name))
            {
                throw new CommandArgumentException(
                    $"Missing required option '{option.Name}'.");
            }
        }
    }

    private List<OptionInfo> GetOptions()
    {
        var options = new List<OptionInfo>(_globalOptions.Count + 2);

        foreach (var option in _globalOptions)
        {
            options.Add(new OptionInfo(
                option.Names,
                option.Description,
                option.Required,
                option.TakesValue,
                option.DeclaredType));
        }

        if (_helpProviderFactory is not null) options.Add(new OptionInfo(["-h", "--help"], "Prints help information"));
        if (_version is not null) options.Add(new OptionInfo(["-v", "--version"], "Prints version information"));

        return options;
    }
}
