// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Provides default English messages for command-line errors.
/// </summary>
/// <remarks>
/// Derive from this class and override individual methods to customise or localise messages.
/// </remarks>
public class CommandErrorMessageProvider : ICommandErrorMessageProvider
{
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
        "No command was specified.";

    /// <summary>
    /// Gets the message for an unknown command.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        UnknownCommandError error) =>
        $"Unknown command '{error.AttemptedCommandPath}'.";

    /// <summary>
    /// Gets the message for a missing subcommand.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingSubcommandError error) =>
        $"Command '{error.CommandPath}' requires a subcommand.";

    /// <summary>
    /// Gets the message for an unknown option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        UnknownOptionError error) =>
        $"Unknown option '{error.OptionName}'.";

    /// <summary>
    /// Gets the message for a malformed option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MalformedOptionError error) =>
        $"Invalid option '{error.Token}'.";

    /// <summary>
    /// Gets the message for a value-taking option in a short-option cluster.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        OptionRequiresSeparateValueError error) =>
        $"Option '{error.OptionName}' requires a separate value; " +
        $"use '{error.OptionName} value' or '{error.OptionName}=value'.";

    /// <summary>
    /// Gets the message for a missing option value.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingOptionValueError error) =>
        $"Missing value for option '{error.OptionName}'.";

    /// <summary>
    /// Gets the message for a repeated scalar option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        RepeatedOptionError error) =>
        $"Option '{error.OptionName}' cannot be specified more than once.";

    /// <summary>
    /// Gets the message for a missing required option.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingRequiredOptionError error) =>
        $"Missing required option '{error.OptionName}'.";

    /// <summary>
    /// Gets the message for a missing required argument.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        MissingRequiredArgumentError error) =>
        $"Missing required argument '{error.ArgumentName}'.";

    /// <summary>
    /// Gets the message for an unexpected argument.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        UnexpectedArgumentError error) =>
        $"Unexpected argument '{error.Value}'.";

    /// <summary>
    /// Gets the message for too few variadic values.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        TooFewVariadicValuesError error) =>
        $"Argument '{error.ArgumentName}' requires at least {error.Minimum} values; " +
        $"received {error.Actual}.";

    /// <summary>
    /// Gets the message for too many variadic values.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        TooManyVariadicValuesError error) =>
        $"Argument '{error.ArgumentName}' accepts at most {error.Maximum} values; " +
        $"received {error.Actual}.";

    /// <summary>
    /// Gets the message for an invalid parameter value.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <param name="error">The command error.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetMessage(
        CommandErrorContext context,
        InvalidParameterValueError error) =>
        $"Invalid value '{error.Value}' for parameter '{error.ParameterName}'. " +
        $"Expected {error.TargetType.Name}.";

    /// <summary>
    /// Gets the fallback message for an unhandled error type.
    /// </summary>
    /// <param name="context">The error context.</param>
    /// <returns>The error message.</returns>
    protected virtual string GetFallbackMessage(CommandErrorContext context) =>
        context.Error.ToString() ?? "Unexpected error occurred.";
}
