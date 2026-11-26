// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Commands.Metadata;
using AnyCli.Parameters.Arguments;
using AnyCli.Parameters.Options;

using Spectre.Console;

namespace AnyCli.SpectreConsole;

/// <summary>
/// Writes styled help output with Spectre.Console.
/// </summary>
public sealed class SpectreHelpProvider : IHelpProvider
{
    private readonly SpectreHelpOptions _options;
    private readonly IHelpTextProvider _textProvider;
    private readonly IHelpValueFormatter _valueFormatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectreHelpProvider"/> class.
    /// </summary>
    /// <param name="textProvider">The provider for localized help text.</param>
    /// <param name="valueFormatter">The formatter for default values.</param>
    /// <param name="options">The styling options, or <see langword="null"/> to use defaults.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textProvider"/> is <see langword="null"/>.
    /// </exception>
    public SpectreHelpProvider(
        IHelpTextProvider textProvider,
        IHelpValueFormatter? valueFormatter = null,
        SpectreHelpOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(textProvider);
        _textProvider = textProvider;
        _valueFormatter = valueFormatter ?? new DefaultHelpValueFormatter();
        _options = options ?? new SpectreHelpOptions();
    }

    /// <inheritdoc/>
    public ValueTask WriteRootHelpAsync(
        RootHelpContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        var output = context.AnsiConsole.Output;
        var hasCommands = context.Commands.Count > 0;
        var usage = CreateRootUsage(context.AppName, hasCommands);

        WriteUsage(output, _textProvider.GetUsageHeading(), usage);

        WriteEntries(
            output,
            _textProvider.GetOptionsHeading(),
            [.. context.GlobalOptions.Select(CreateOptionEntry)]);

        if (hasCommands)
        {
            WriteEntries(
                output,
                _textProvider.GetCommandsHeading(),
                [.. context.Commands.Select(CreateCommandEntry)]);
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public ValueTask WriteCommandHelpAsync(
        CommandHelpContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var command = context.Command;
        var output = context.AnsiConsole.Output;
        var description = ResolveDescription(command, command.Description);

        if (!string.IsNullOrEmpty(description))
        {
            WriteDescription(
                output,
                _textProvider.GetDescriptionHeading(),
                description);
            output.WriteLine();
        }

        WriteUsage(
            output,
            _textProvider.GetUsageHeading(),
            CreateCommandUsage(context.AppName, command));

        if (command.Arguments.Count > 0)
        {
            WriteEntries(
                output,
                _textProvider.GetArgumentsHeading(),
                [.. command.Arguments.Select(argument => CreateArgumentEntry(command, argument))]);
        }

        if (command.Options.Count > 0 || context.GlobalOptions.Count > 0)
        {
            WriteEntries(
                output,
                _textProvider.GetOptionsHeading(),
                [
                    .. command.Options.Select(CreateOptionEntry),
                    .. context.GlobalOptions.Select(CreateOptionEntry),
                ]);
        }

        if (command.Children.Count > 0)
        {
            WriteEntries(
                output,
                _textProvider.GetCommandsHeading(),
                [.. command.Children.Select(CreateCommandEntry)]);
        }

        return ValueTask.CompletedTask;
    }

    private List<UsageSegment> CreateRootUsage(string appName, bool hasCommands)
    {
        List<UsageSegment> usage =
        [
            new(appName, _options.UsageStyle),
            new(" ", _options.UsageStyle),
            new(GetOptionsUsage(), _options.OptionsPlaceholderStyle),
        ];

        if (hasCommands)
        {
            usage.Add(new(" ", _options.UsageStyle));
            usage.Add(new(GetCommandUsage(), _options.CommandPlaceholderStyle));
        }

        return usage;
    }

    private List<UsageSegment> CreateCommandUsage(string appName, CommandInfo command)
    {
        List<UsageSegment> parts =
        [
            new(appName, _options.UsageStyle),
            new(" ", _options.UsageStyle),
            new(
                command.Path.Count == 0 ? command.Name : string.Join(' ', command.Path),
                _options.UsageStyle),
        ];

        foreach (var argument in command.Arguments)
        {
            parts.Add(new(" ", _options.UsageStyle));
            parts.Add(new(GetArgumentSyntax(argument), _options.ArgumentStyle));
        }

        parts.Add(new(" ", _options.UsageStyle));
        parts.Add(new(GetOptionsUsage(), _options.OptionsPlaceholderStyle));

        if (command.Children.Count > 0)
        {
            parts.Add(new(" ", _options.UsageStyle));
            parts.Add(new(GetCommandUsage(), _options.CommandPlaceholderStyle));
        }

        return parts;
    }

    private string GetOptionsUsage() => $"[{_textProvider.GetOptionsPlaceholder()}]";

    private string GetCommandUsage() => $"<{_textProvider.GetCommandPlaceholder()}>";

    private static string GetArgumentSyntax(ArgumentInfo argument)
    {
        var name = argument.Arity.IsVariadic
            ? argument.Name + "..."
            : argument.Name;

        return argument.IsRequired
            ? $"<{name}>"
            : $"[{name}]";
    }

    private HelpEntry CreateArgumentEntry(CommandInfo command, ArgumentInfo argument) =>
        new(GetArgumentSyntax(argument),
            ResolveDescription(command, argument.Description, argument: argument),
            _options.ArgumentStyle);

    private HelpEntry CreateOptionEntry(OptionInfo option) =>
        new(string.Join(", ", option.Names),
            ResolveDescription(command: null, option.Description, option: option),
            _options.OptionStyle);

    private HelpEntry CreateCommandEntry(CommandInfo command) =>
        new(command.Name,
            ResolveDescription(command, command.Description),
            _options.CommandStyle);

    private string ResolveDescription(
        CommandInfo? command,
        HelpText description,
        OptionInfo? option = null,
        ArgumentInfo? argument = null)
    {
        var resolved = _textProvider.GetDescription(
            new HelpDescriptionContext(
                command,
                description,
                option,
                argument));

        var text = GetDefaultValueText(option, argument);
        if (text is null) return resolved;

        var defaultValue = $"[{_textProvider.GetDefaultValueLabel()}: {text}]";
        return resolved.Length == 0 ? defaultValue : $"{resolved} {defaultValue}";
    }

    private string? GetDefaultValueText(OptionInfo? option, ArgumentInfo? argument)
    {
        var display = option?.DefaultValue?.DisplayText ?? argument?.DefaultValue?.DisplayText ?? HelpText.Empty;
        if (!display.IsEmpty)
        {
            var displayText = _textProvider.GetDefaultValueText(display);
            if (!string.IsNullOrWhiteSpace(displayText)) return displayText;
        }

        return option?.DefaultValue is DefaultValueInfo.Provider || argument?.DefaultValue is DefaultValueInfo.Provider
            ? null
            : option?.DefaultValue is DefaultValueInfo.Initializer optionInitializer
                ? _valueFormatter.Format(optionInitializer.Value, _textProvider.GetFormatCulture())
                : argument?.DefaultValue is DefaultValueInfo.Initializer argumentInitializer
                    ? _valueFormatter.Format(argumentInitializer.Value, _textProvider.GetFormatCulture())
                    : null;
    }

    private void WriteDescription(
        IAnsiConsole output,
        string heading,
        string description)
    {
        WriteLine(output, $"{heading}:", _options.HeadingStyle);
        WriteLine(output, $"    {description}", _options.DescriptionStyle);
    }

    private void WriteUsage(
        IAnsiConsole output,
        string heading,
        IReadOnlyList<UsageSegment> usage)
    {
        WriteLine(output, $"{heading}:", _options.HeadingStyle);
        output.Write(new Text("    ", _options.UsageStyle));

        foreach (var segment in usage) output.Write(new Text(segment.Text, segment.Style));

        output.WriteLine();
    }

    private void WriteEntries(
        IAnsiConsole output,
        string heading,
        IReadOnlyList<HelpEntry> entries)
    {
        if (entries.Count == 0) return;

        output.WriteLine();
        WriteLine(output, $"{heading}:", _options.HeadingStyle);

        var width = entries.Max(entry => entry.Syntax.Length);
        var hasDescription = entries.Any(entry => entry.Description.Length > 0);

        foreach (var entry in entries)
        {
            output.Write(new Text("    "));
            output.Write(new Text(entry.Syntax, entry.Style));

            if (hasDescription)
            {
                output.Write(new Text(new string(' ', width - entry.Syntax.Length + 2)));
                output.Write(new Text(entry.Description, _options.DescriptionStyle));
            }

            output.WriteLine();
        }
    }

    private static void WriteLine(
        IAnsiConsole output,
        string value,
        Style style)
    {
        output.Write(new Text(value, style));
        output.WriteLine();
    }

    private readonly record struct HelpEntry(
        string Syntax,
        string Description,
        Style Style);

    private readonly record struct UsageSegment(string Text, Style Style);
}
