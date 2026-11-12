// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// The source of a command parameter value.
/// </summary>
public enum ParameterSource
{
    /// <summary>
    /// The source is not known.
    /// </summary>
    Unknown,

    /// <summary>
    /// The value came from a positional argument.
    /// </summary>
    Argument,

    /// <summary>
    /// The value came from a command option.
    /// </summary>
    Option,

    /// <summary>
    /// The value came from a global option.
    /// </summary>
    GlobalOption,
}

/// <summary>
/// Represents an error caused by invalid command-line input.
/// </summary>
public abstract record CommandError;

/// <summary>
/// No command was specified.
/// </summary>
public sealed record NoCommandSpecifiedError : CommandError;

/// <summary>
/// A command was not recognized.
/// </summary>
/// <param name="AttemptedCommandPath">The command path that was attempted.</param>
/// <param name="AvailableCommands">The commands available at the current level.</param>
public sealed record UnknownCommandError(
    string AttemptedCommandPath,
    IReadOnlyList<string> AvailableCommands) : CommandError;

/// <summary>
/// A command was recognized but requires a subcommand.
/// </summary>
/// <param name="CommandPath">The command path that requires a subcommand.</param>
/// <param name="AvailableCommands">The commands available at the current level.</param>
public sealed record MissingSubcommandError(
    string CommandPath,
    IReadOnlyList<string> AvailableCommands) : CommandError;

/// <summary>
/// An option name was not recognized.
/// </summary>
/// <param name="OptionName">The unrecognized option name.</param>
public sealed record UnknownOptionError(string OptionName) : CommandError;

/// <summary>
/// An option token was malformed.
/// </summary>
/// <param name="Token">The malformed option token.</param>
public sealed record MalformedOptionError(string Token) : CommandError;

/// <summary>
/// A value-taking option was used in a short-option cluster.
/// </summary>
/// <param name="OptionName">The option name.</param>
public sealed record OptionRequiresSeparateValueError(string OptionName) : CommandError;

/// <summary>
/// An option that requires a value was supplied without one.
/// </summary>
/// <param name="OptionName">The option name.</param>
public sealed record MissingOptionValueError(string OptionName) : CommandError;

/// <summary>
/// A scalar option was specified more than once.
/// </summary>
/// <param name="OptionName">The option name.</param>
public sealed record RepeatedOptionError(string OptionName) : CommandError;

/// <summary>
/// A required option was not supplied.
/// </summary>
/// <param name="OptionName">The option name.</param>
/// <param name="IsGlobal">Whether the option is a global option.</param>
public sealed record MissingRequiredOptionError(
    string OptionName,
    bool IsGlobal) : CommandError;

/// <summary>
/// A required positional argument was not supplied.
/// </summary>
/// <param name="ArgumentName">The argument name.</param>
public sealed record MissingRequiredArgumentError(string ArgumentName) : CommandError;

/// <summary>
/// A positional value could not be assigned to any argument.
/// </summary>
/// <param name="Value">The unexpected value.</param>
public sealed record UnexpectedArgumentError(string Value) : CommandError;

/// <summary>
/// A variadic argument received fewer values than its minimum.
/// </summary>
/// <param name="ArgumentName">The argument name.</param>
/// <param name="Minimum">The minimum number of values.</param>
/// <param name="Actual">The actual number of supplied values.</param>
public sealed record TooFewVariadicValuesError(
    string ArgumentName,
    int Minimum,
    int Actual) : CommandError;

/// <summary>
/// A variadic argument received more values than its maximum.
/// </summary>
/// <param name="ArgumentName">The argument name.</param>
/// <param name="Maximum">The maximum number of values.</param>
/// <param name="Actual">The actual number of supplied values.</param>
public sealed record TooManyVariadicValuesError(
    string ArgumentName,
    int Maximum,
    int Actual) : CommandError;

/// <summary>
/// A parameter value could not be converted to its declared type.
/// </summary>
/// <param name="ParameterName">The parameter name.</param>
/// <param name="Value">The invalid value.</param>
/// <param name="TargetType">The expected type.</param>
/// <param name="Source">The source of the value.</param>
public sealed record InvalidParameterValueError(
    string ParameterName,
    string Value,
    Type TargetType,
    ParameterSource Source = ParameterSource.Unknown) : CommandError;
