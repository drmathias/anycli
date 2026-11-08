// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Globalization;

namespace AnyCli;

/// <summary>
/// A configured command-line application.
/// </summary>
/// <remarks>
/// Instances are created with <see cref="ConsoleAppBuilder.Build"/>.
/// </remarks>
public sealed class ConsoleApp
{
    private readonly IReadOnlyList<OptionDefinition> _globalOptions;
    private readonly Func<IServiceProvider?, IHelpProvider>? _helpProviderFactory;
    private readonly IReadOnlyList<Func<IServiceProvider?, ICommandInterceptor>> _interceptorFactories;
    private readonly string? _name;
    private readonly CommandNode _root;
    private readonly IServiceProvider? _serviceProvider;
    private readonly Func<IServiceProvider?, IUnrecognizedCommandHandler> _unrecognizedCommandHandlerFactory;
    private readonly int _unrecognizedCommandExitCode;
    private readonly string? _version;

    internal ConsoleApp(
        string? name,
        string? version,
        CommandNode root,
        IReadOnlyList<OptionDefinition> globalOptions,
        IServiceProvider? serviceProvider,
        Func<IServiceProvider?, IHelpProvider>? helpProviderFactory,
        Func<IServiceProvider?, IUnrecognizedCommandHandler> unrecognizedCommandHandlerFactory,
        int unrecognizedCommandExitCode,
        IReadOnlyList<Func<IServiceProvider?, ICommandInterceptor>> interceptorFactories)
    {
        _name = name;
        _version = version;
        _root = root;
        _globalOptions = globalOptions;
        _serviceProvider = serviceProvider;
        _helpProviderFactory = helpProviderFactory;
        _unrecognizedCommandHandlerFactory = unrecognizedCommandHandlerFactory;
        _unrecognizedCommandExitCode = unrecognizedCommandExitCode;
        _interceptorFactories = interceptorFactories;
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
    /// <paramref name="args"/> or <paramref name="console"/> are <see langword="null"/>.
    /// </exception>
    public async Task<int> RunAsync(string[] args, IConsole console)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(console);

        var outputWriter = console.Output;
        var globalOptionValues = new Dictionary<string, OptionValue>(StringComparer.Ordinal);
        var commandIndex = CommandParameterParser.ParseLeadingGlobalOptions(
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

        if (commandIndex < 0)
        {
            return HandleUnrecognized(
                console,
                string.Empty,
                GetChildPaths(_root, []),
                isMissingSubcommand: false);
        }

        var path = new List<string>();
        var current = _root;
        var index = commandIndex;

        while (true)
        {
            if (index >= args.Length)
            {
                return current.Command is not null
                    ? await ExecuteCommandAsync(
                        current,
                        path,
                        args,
                        args.Length,
                        console,
                        globalOptionValues).ConfigureAwait(false)
                    : HandleUnrecognized(
                        console,
                        string.Join(' ', path),
                        GetChildPaths(current, path),
                        isMissingSubcommand: true);
            }

            var token = args[index];
            if (!current.TryGetChild(token, out var child))
            {
                var attemptedPath = new List<string>(path) { token };
                return HandleUnrecognized(
                    console,
                    string.Join(' ', attemptedPath),
                    GetChildPaths(current, path),
                    isMissingSubcommand: false);
            }

            path.Add(token);

            if (child.Children.Count == 0)
            {
                return await ExecuteCommandAsync(
                    child,
                    path,
                    args,
                    index + 1,
                    console,
                    globalOptionValues).ConfigureAwait(false);
            }

            if (child.Command is not null)
            {
                var childRelativeIndex = CommandParameterParser.FindFirstNonGlobalOption(
                    _globalOptions,
                    args.AsSpan(index + 1),
                    globalOptionValues,
                    _helpProviderFactory is not null,
                    _version is not null,
                    out var commandHelpRequested,
                    out var commandVersionRequested);

                if (commandVersionRequested)
                {
                    outputWriter.WriteLine(_version);
                    return 0;
                }

                if (commandHelpRequested)
                {
                    WriteCommandHelp(console, child, path);
                    return 0;
                }

                if (childRelativeIndex >= 0)
                {
                    var absoluteIndex = index + 1 + childRelativeIndex;
                    var nextToken = args[absoluteIndex];

                    if (child.TryGetChild(nextToken, out _))
                    {
                        current = child;
                        index = absoluteIndex;
                        continue;
                    }

                    if (nextToken.Length > 0 && nextToken[0] != '-')
                    {
                        var attemptedPath = new List<string>(path) { nextToken };
                        return HandleUnrecognized(
                            console,
                            string.Join(' ', attemptedPath),
                            GetChildPaths(child, path),
                            isMissingSubcommand: false);
                    }

                    return await ExecuteCommandAsync(
                        child,
                        path,
                        args,
                        absoluteIndex,
                        console,
                        globalOptionValues).ConfigureAwait(false);
                }

                return await ExecuteCommandAsync(
                    child,
                    path,
                    args,
                    args.Length,
                    console,
                    globalOptionValues).ConfigureAwait(false);
            }

            var relativeIndex = CommandParameterParser.ParseLeadingGlobalOptions(
                _globalOptions,
                args.AsSpan(index + 1),
                globalOptionValues,
                _helpProviderFactory is not null,
                _version is not null,
                out var groupHelpRequested,
                out var groupVersionRequested);

            if (groupVersionRequested)
            {
                outputWriter.WriteLine(_version);
                return 0;
            }

            if (groupHelpRequested)
            {
                WriteCommandHelp(console, child, path);
                return 0;
            }

            if (relativeIndex < 0)
            {
                return HandleUnrecognized(
                    console,
                    string.Join(' ', path),
                    GetChildPaths(child, path),
                    isMissingSubcommand: true);
            }

            current = child;
            index = index + 1 + relativeIndex;
        }
    }

    private async Task<int> ExecuteCommandAsync(
        CommandNode node,
        IReadOnlyList<string> path,
        string[] args,
        int startIndex,
        IConsole console,
        Dictionary<string, OptionValue> globalOptionValues)
    {
        var command = node.Command!;
        var arguments = CommandParameterParser.Parse(
            command.Arguments,
            command.Options,
            _globalOptions,
            args.AsSpan(startIndex),
            globalOptionValues,
            _helpProviderFactory is not null,
            _version is not null,
            out var helpRequested,
            out var versionRequested);

        if (versionRequested)
        {
            console.Output.WriteLine(_version);
            return 0;
        }

        if (helpRequested)
        {
            WriteCommandHelp(console, node, path);
            return 0;
        }

        ValidateRequiredGlobalOptions(globalOptionValues);

        var context = new CommandContext(
            path,
            console,
            CreateGlobalOptionValues(globalOptionValues));

        return await ExecuteWithInterceptorsAsync(
                context,
                () => command.Execute.Invoke(_serviceProvider, context, arguments!))
            .ConfigureAwait(false);
    }

    private Task<int> ExecuteWithInterceptorsAsync(
        CommandContext context,
        Func<Task<int>> next)
    {
        if (_interceptorFactories.Count == 0)
        {
            return next();
        }

        var interceptors = new ICommandInterceptor[_interceptorFactories.Count];
        for (var index = 0; index < interceptors.Length; index++)
        {
            interceptors[index] = _interceptorFactories[index](_serviceProvider);
        }

        var pipeline = next;
        for (var index = interceptors.Length - 1; index >= 0; index--)
        {
            var interceptor = interceptors[index];
            var inner = pipeline;
            pipeline = () => interceptor.InterceptAsync(context, inner);
        }

        return pipeline();
    }

    private static CommandInfo CreateCommandInfo(
        CommandNode node,
        IReadOnlyList<string> parentPath)
    {
        var path = new List<string>(parentPath.Count + 1);
        path.AddRange(parentPath);
        path.Add(node.Name);

        var command = node.Command;
        return new CommandInfo(node.Name, node.Description)
        {
            Path = path,
            IsExecutable = command is not null,
            HasSubcommands = node.Children.Count > 0,
            Arguments = command is null
                ? []
                : [.. command.Arguments
                    .Select(argument => new ArgumentInfo(
                        argument.Name,
                        argument.Description,
                        argument.DeclaredType,
                        argument.Arity)
                    {
                        DefaultValue = argument.DefaultValue,
                        HasDefaultValue = argument.HasDefaultValue,
                    })],
            Options = command is null
                ? []
                : [.. command.Options
                    .Select(option => new OptionInfo(
                        option.Names,
                        option.Description,
                        option.Required,
                        option.TakesValue,
                        option.DeclaredType)
                    {
                        DefaultValue = option.DefaultValue,
                        HasDefaultValue = option.HasDefaultValue,
                    })],
            Children = [.. node.Children.Select(child => CreateCommandInfo(child, path))],
        };
    }

    private static IReadOnlyList<string> GetChildPaths(
        CommandNode node,
        IReadOnlyList<string> parentPath) =>
        [.. node.Children.Select(child => string.Join(' ', parentPath.Append(child.Name)))];

    private int HandleUnrecognized(
        IConsole console,
        string commandName,
        IReadOnlyList<string> availableCommands,
        bool isMissingSubcommand)
    {
        var context = new UnrecognizedCommandContext(
            _name,
            commandName,
            availableCommands,
            console)
        {
            IsMissingSubcommand = isMissingSubcommand,
        };

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
                [.. _root.Children.Select(child => CreateCommandInfo(child, []))]));
    }

    private void WriteCommandHelp(
        IConsole console,
        CommandNode command,
        IReadOnlyList<string> commandPath)
    {
        _helpProviderFactory!(_serviceProvider).WriteCommandHelp(
            new CommandHelpContext(
                _name ?? string.Empty,
                console,
                GetOptions(),
                CreateCommandInfo(command, [.. commandPath.Take(commandPath.Count - 1)])));
    }

    private CommandOptionValues CreateGlobalOptionValues(
        IReadOnlyDictionary<string, OptionValue> globalOptionValues)
    {
        var defaults = new Dictionary<string, OptionValue>(StringComparer.Ordinal);
        foreach (var option in _globalOptions)
        {
            if (!option.HasDefaultValue || globalOptionValues.ContainsKey(option.Name))
            {
                continue;
            }

            var defaultValue = new OptionValue();
            AddDefaultValues(defaultValue, option.DefaultValue);
            defaults[option.Name] = defaultValue;
        }

        return new CommandOptionValues(globalOptionValues, defaults);
    }

    private static void AddDefaultValues(OptionValue optionValue, object? value)
    {
        if (value is null)
        {
            return;
        }

        if (value is string stringValue)
        {
            optionValue.Values.Add(stringValue);
            return;
        }

        if (value is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                AddDefaultValues(optionValue, item);
            }

            return;
        }

        var text = Convert.ToString(value, CultureInfo.InvariantCulture);
        if (text is not null)
        {
            optionValue.Values.Add(text);
        }
    }

    private void ValidateRequiredGlobalOptions(
        IReadOnlyDictionary<string, OptionValue> globalOptionValues)
    {
        foreach (var option in _globalOptions)
        {
            if (option.Required && !globalOptionValues.ContainsKey(option.Name))
            {
                throw new CommandParameterException(
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
                option.DeclaredType)
            {
                DefaultValue = option.DefaultValue,
                HasDefaultValue = option.HasDefaultValue,
            });
        }

        if (_helpProviderFactory is not null) options.Add(new OptionInfo(["-h", "--help"], "Prints help information"));
        if (_version is not null) options.Add(new OptionInfo(["-v", "--version"], "Prints version information"));

        return options;
    }
}
