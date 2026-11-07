// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
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

    /// <summary>
    /// Runs the application using the console for output.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The process exit code.</returns>
    public Task<int> RunAsync(string[] args) => RunAsync(args, Console.Out, Console.Error);

    /// <summary>
    /// Runs the application using the specified writers.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="output">The writer for standard output.</param>
    /// <param name="error">The writer for error output.</param>
    /// <returns>The process exit code.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="args"/>, <paramref name="output"/>, or <paramref name="error"/> is <see langword="null"/>.
    /// </exception>
    public async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        var outputWriter = new TextWriterOutputWriter(output);
        var errorWriter = new TextWriterOutputWriter(error);

        if (IsVersionRequest(args))
        {
            outputWriter.WriteLine(_version);
            return 0;
        }

        if (_helpProvider is not null && IsHelpRequest(args))
        {
            var commandName = args.Length == 2 ? args[0] : null;
            _helpProvider.WriteHelp(new HelpContext(_name ?? string.Empty, outputWriter, commandName, _commandInfos, GetOptions()));
            return 0;
        }

        return args.Length == 1 && _commands.TryGetValue(args[0], out var execute)
            ? await execute.Invoke(_serviceProvider, new CommandContext(args[0], outputWriter, errorWriter)).ConfigureAwait(false)
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
