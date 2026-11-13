// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Resources;

namespace AnyCli.Errors;

/// <summary>
/// Provides resource-based, localized messages for command-line errors.
/// </summary>
/// <remarks>
/// Derive from this class and override individual methods to customise or localise messages.
/// </remarks>
public class LocalizedCommandErrorMessageProvider : ICommandErrorMessageProvider
{
    private const string ResourceBaseName = "AnyCli.Errors.CommandErrorMessages";

    private static readonly ResourceManager Resources = new(
        ResourceBaseName,
        typeof(LocalizedCommandErrorMessageProvider).Assembly);

    /// <inheritdoc/>
    public string GetMessage(CommandErrorContext context) =>
        context.Error switch
        {
            NoCommandSpecifiedError error => GetMessage(context, error),
            UnknownCommandError error => GetMessage(context, error),
            MissingSubcommandError error => GetMessage(context, error),
            UnknownOptionError error => GetMessage(context, error),
            MalformedOptionError error => GetMessage(context, error),
            OptionRequiresSeparateValueError error => GetMessage(context, error),
            MissingOptionValueError error => GetMessage(context, error),
            RepeatedOptionError error => GetMessage(context, error),
            MissingRequiredOptionError error => GetMessage(context, error),
            MissingRequiredArgumentError error => GetMessage(context, error),
            UnexpectedArgumentError error => GetMessage(context, error),
            TooFewVariadicValuesError error => GetMessage(context, error),
            TooManyVariadicValuesError error => GetMessage(context, error),
            InvalidParameterValueError error => GetMessage(context, error),
            _ => GetFallbackMessage(context),
        };

    /// <summary>
    /// Gets the message for a missing command.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        NoCommandSpecifiedError error) =>
        Format(context, CommandErrorResourceKeys.NoCommandSpecified);

    /// <summary>
    /// Gets the message for an unknown command.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        UnknownCommandError error) =>
        Format(context, CommandErrorResourceKeys.UnknownCommand, error.AttemptedCommandPath);

    /// <summary>
    /// Gets the message for a missing subcommand.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingSubcommandError error) =>
        Format(context, CommandErrorResourceKeys.MissingSubcommand, error.CommandPath);

    /// <summary>
    /// Gets the message for an unknown option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        UnknownOptionError error) =>
        Format(context, CommandErrorResourceKeys.UnknownOption, error.OptionName);

    /// <summary>
    /// Gets the message for a malformed option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MalformedOptionError error) =>
        Format(context, CommandErrorResourceKeys.MalformedOption, error.Token);

    /// <summary>
    /// Gets the message for a value-taking option in a short-option cluster.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        OptionRequiresSeparateValueError error) =>
        Format(
            context,
            CommandErrorResourceKeys.OptionRequiresSeparateValue,
            error.OptionName);

    /// <summary>
    /// Gets the message for a missing option value.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingOptionValueError error) =>
        Format(context, CommandErrorResourceKeys.MissingOptionValue, error.OptionName);

    /// <summary>
    /// Gets the message for a repeated scalar option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        RepeatedOptionError error) =>
        Format(context, CommandErrorResourceKeys.RepeatedOption, error.OptionName);

    /// <summary>
    /// Gets the message for a missing required option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingRequiredOptionError error) =>
        Format(context, CommandErrorResourceKeys.MissingRequiredOption, error.OptionName);

    /// <summary>
    /// Gets the message for a missing required argument.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingRequiredArgumentError error) =>
        Format(context, CommandErrorResourceKeys.MissingRequiredArgument, error.ArgumentName);

    /// <summary>
    /// Gets the message for an unexpected argument.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        UnexpectedArgumentError error) =>
        Format(context, CommandErrorResourceKeys.UnexpectedArgument, error.Value);

    /// <summary>
    /// Gets the message for too few variadic values.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        TooFewVariadicValuesError error) =>
        Format(
            context,
            CommandErrorResourceKeys.TooFewVariadicValues,
            error.ArgumentName,
            error.Minimum,
            error.Actual);

    /// <summary>
    /// Gets the message for too many variadic values.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        TooManyVariadicValuesError error) =>
        Format(
            context,
            CommandErrorResourceKeys.TooManyVariadicValues,
            error.ArgumentName,
            error.Maximum,
            error.Actual);

    /// <summary>
    /// Gets the message for an invalid parameter value.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        InvalidParameterValueError error) =>
        Format(
            context,
            CommandErrorResourceKeys.InvalidParameterValue,
            error.Value,
            error.ParameterName,
            error.TargetType.Name);

    /// <summary>
    /// Gets the fallback message for an unhandled error type.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetFallbackMessage(CommandErrorContext context) =>
        context.Error.ToString() ?? "Unexpected error occurred.";

    /// <summary>
    /// Gets the UI culture used to select error messages.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <returns>The UI culture.</returns>
    protected virtual CultureInfo GetUICulture(CommandErrorContext context) =>
        CultureInfo.CurrentUICulture;

    private string Format(
        CommandErrorContext context,
        string resourceKey,
        params object?[] arguments)
    {
        var template = Resources.GetString(resourceKey, GetUICulture(context))
            ?? resourceKey;

        return string.Format(CultureInfo.CurrentCulture, template, arguments);
    }
}
