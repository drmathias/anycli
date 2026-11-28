// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Writes plain-text help output.
/// </summary>
public sealed class DefaultHelpRenderer : IHelpRenderer
{
    private readonly IHelpTextProvider _textProvider;
    private readonly IHelpValueFormatter _valueFormatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHelpRenderer"/> class.
    /// </summary>
    /// <param name="textProvider">The provider for localized help text.</param>
    /// <param name="valueFormatter">The formatter for default values.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textProvider"/> is <see langword="null"/>.
    /// </exception>
    public DefaultHelpRenderer(
        IHelpTextProvider textProvider,
        IHelpValueFormatter? valueFormatter = null)
    {
        ArgumentNullException.ThrowIfNull(textProvider);
        _textProvider = textProvider;
        _valueFormatter = valueFormatter ?? new DefaultHelpValueFormatter();
    }

    /// <inheritdoc/>
    public ValueTask WriteRootHelpAsync(
        RootHelpContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var output = context.Console.Output;
        var hasCommands = context.Application.Commands.Count > 0;
        var options = GetOptionsUsage();
        var usage = hasCommands
            ? $"{context.Application.AppName} {options} {GetCommandUsage()}"
            : $"{context.Application.AppName} {options}";

        WriteUsage(output, _textProvider.GetUsageHeading(), usage);

        WriteEntries(
            output,
            _textProvider.GetOptionsHeading(),
            [
                .. context.Application.GlobalOptions.Select(option => CreateOptionEntry(command: null, option)),
                .. context.Application.FrameworkOptions.Select(option =>
                    CreateFrameworkOptionEntry(command: null, option)),
            ]);

        if (hasCommands)
        {
            WriteEntries(
                output,
                _textProvider.GetCommandsHeading(),
                [.. context.Application.Commands.Select(command =>
                    new HelpEntry(command.Name, ResolveDescription(command, command.Description)))]);
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
        var output = context.Console.Output;

        var description = ResolveDescription(command, command.Description);
        if (!string.IsNullOrEmpty(description))
        {
            WriteDescription(output, _textProvider.GetDescriptionHeading(), description);
            output.WriteLine();
        }

        WriteUsage(output, _textProvider.GetUsageHeading(), CreateCommandUsage(context.Application.AppName, command));

        if (command.Arguments.Count > 0)
        {
            WriteEntries(
                output,
                _textProvider.GetArgumentsHeading(),
                [.. command.Arguments.Select(argument => CreateArgumentEntry(command, argument))]);
        }

        if (command.Options.Count > 0 ||
            context.Application.GlobalOptions.Count > 0 ||
            context.Application.FrameworkOptions.Count > 0)
        {
            WriteEntries(
                output,
                _textProvider.GetOptionsHeading(),
                [
                    .. command.Options.Select(option => CreateOptionEntry(command, option)),
                    .. context.Application.GlobalOptions.Select(option => CreateOptionEntry(command, option)),
                    .. context.Application.FrameworkOptions.Select(option =>
                        CreateFrameworkOptionEntry(command, option)),
                ]);
        }

        if (command.Children.Count > 0)
        {
            WriteEntries(
                output,
                _textProvider.GetCommandsHeading(),
                [.. command.Children.Select(child =>
                    new HelpEntry(child.Name, ResolveDescription(child, child.Description)))]);
        }

        return ValueTask.CompletedTask;
    }

    private string CreateCommandUsage(string appName, CommandInfo command)
    {
        var parts = new List<string>(5)
        {
            appName,
            command.Path.Count == 0 ? command.Name : string.Join(' ', command.Path),
        };

        foreach (var argument in command.Arguments) parts.Add(GetArgumentSyntax(argument));

        parts.Add(GetOptionsUsage());

        if (command.Children.Count > 0) parts.Add(GetCommandUsage());

        return string.Join(' ', parts);
    }

    private string GetOptionsUsage() => $"[{_textProvider.GetOptionsPlaceholder()}]";

    private string GetCommandUsage() => $"<{_textProvider.GetCommandPlaceholder()}>";

    private static string GetArgumentSyntax(ArgumentInfo argument)
    {
        var name = argument.Arity.IsVariadic ? argument.Name + "..." : argument.Name;
        return argument.IsRequired ? $"<{name}>" : $"[{name}]";
    }

    private HelpEntry CreateArgumentEntry(CommandInfo command, ArgumentInfo argument) =>
        new(GetArgumentSyntax(argument),
            ResolveDescription(command, argument.Description, argument: argument));

    private HelpEntry CreateOptionEntry(CommandInfo? command, OptionInfo option) =>
        new(string.Join(", ", option.Names),
            ResolveDescription(command, option.Description, option: option));

    private HelpEntry CreateFrameworkOptionEntry(CommandInfo? command, FrameworkOptionInfo option) =>
        new(string.Join(", ", option.Names),
            ResolveDescription(command, option.Description));

    private string ResolveDescription(
        CommandInfo? command,
        HelpText description,
        OptionInfo? option = null,
        ArgumentInfo? argument = null)
    {
        var resolved = _textProvider.GetDescription(new HelpDescriptionContext(command, description, option, argument));

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

    private static void WriteDescription(
        IOutputWriter output,
        string heading,
        string description)
    {
        output.WriteLine($"{heading}:");
        output.WriteLine($"    {description}");
    }

    private static void WriteUsage(IOutputWriter output, string heading, string usage)
    {
        output.WriteLine($"{heading}:");
        output.WriteLine($"    {usage}");
    }

    private static void WriteEntries(
        IOutputWriter output,
        string heading,
        IReadOnlyList<HelpEntry> entries)
    {
        if (entries.Count == 0) return;

        output.WriteLine();
        output.WriteLine($"{heading}:");

        var width = entries.Max(entry => entry.Syntax.Length);
        var hasDescription = entries.Any(entry => entry.Description.Length > 0);

        foreach (var entry in entries)
        {
            output.Write("    ");
            output.Write(entry.Syntax);

            if (hasDescription)
            {
                output.Write(new string(' ', width - entry.Syntax.Length + 2));
                output.Write(entry.Description);
            }

            output.WriteLine();
        }
    }

    private readonly record struct HelpEntry(string Syntax, string Description);
}
