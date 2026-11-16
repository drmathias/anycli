// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Writes plain-text help output.
/// </summary>
public sealed class DefaultHelpProvider : IHelpProvider
{
    private readonly IHelpTextProvider _textProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHelpProvider"/> class.
    /// </summary>
    /// <param name="textProvider">The provider for localized help text.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="textProvider"/> is <see langword="null"/>.
    /// </exception>
    public DefaultHelpProvider(IHelpTextProvider textProvider)
    {
        ArgumentNullException.ThrowIfNull(textProvider);
        _textProvider = textProvider;
    }

    /// <inheritdoc/>
    public ValueTask WriteRootHelpAsync(
        RootHelpContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var output = context.Console.Output;
        var hasCommands = context.Commands.Count > 0;
        var usage = hasCommands
            ? $"{context.AppName} [OPTIONS] <COMMAND>"
            : $"{context.AppName} [OPTIONS]";

        WriteUsage(output, _textProvider.GetUsageHeading(), usage);

        WriteEntries(
            output,
            _textProvider.GetOptionsHeading(),
            [.. context.GlobalOptions.Select(option => CreateOptionEntry(command: null, option))]);

        if (hasCommands)
        {
            WriteEntries(
                output,
                _textProvider.GetCommandsHeading(),
                [.. context.Commands.Select(command =>
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

        WriteUsage(output, _textProvider.GetUsageHeading(), CreateCommandUsage(context.AppName, command));

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
                    .. command.Options.Select(option => CreateOptionEntry(command, option)),
                    .. context.GlobalOptions.Select(option => CreateOptionEntry(command, option)),
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

    private static string CreateCommandUsage(string appName, CommandInfo command)
    {
        var parts = new List<string>(5)
        {
            appName,
            command.Path.Count == 0 ? command.Name : string.Join(' ', command.Path),
        };

        foreach (var argument in command.Arguments)
        {
            parts.Add(GetArgumentSyntax(argument));
        }

        parts.Add("[OPTIONS]");

        if (command.Children.Count > 0)
        {
            parts.Add("<COMMAND>");
        }

        return string.Join(' ', parts);
    }

    private static string GetArgumentSyntax(ArgumentInfo argument)
    {
        var name = argument.IsVariadic
            ? argument.Name + "..."
            : argument.Name;

        return argument.IsRequired
            ? $"<{name}>"
            : $"[{name}]";
    }

    private HelpEntry CreateArgumentEntry(CommandInfo command, ArgumentInfo argument) =>
        new(
            GetArgumentSyntax(argument),
            ResolveDescription(command, argument.Description, argument: argument));

    private HelpEntry CreateOptionEntry(CommandInfo? command, OptionInfo option) =>
        new(
            string.Join(", ", option.Names),
            ResolveDescription(command, option.Description, option: option));

    private string ResolveDescription(
        CommandInfo? command,
        HelpText description,
        OptionInfo? option = null,
        ArgumentInfo? argument = null) =>
        _textProvider.GetDescription(
            new HelpDescriptionContext(
                command,
                description,
                option,
                argument));

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
        if (entries.Count == 0)
        {
            return;
        }

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
