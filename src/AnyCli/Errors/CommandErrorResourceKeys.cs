// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

/// <summary>
/// Defines the resource keys used by the built-in command error resources.
/// </summary>
public static class CommandErrorResourceKeys
{
    /// <summary>
    /// No command was specified.
    /// </summary>
    public const string NoCommandSpecified = nameof(NoCommandSpecified);

    /// <summary>
    /// The command was not recognized.
    /// </summary>
    public const string UnknownCommand = nameof(UnknownCommand);

    /// <summary>
    /// The command requires a subcommand.
    /// </summary>
    public const string MissingSubcommand = nameof(MissingSubcommand);

    /// <summary>
    /// The option was not recognized.
    /// </summary>
    public const string UnknownOption = nameof(UnknownOption);

    /// <summary>
    /// The option token was malformed.
    /// </summary>
    public const string MalformedOption = nameof(MalformedOption);

    /// <summary>
    /// The option requires a separate value.
    /// </summary>
    public const string OptionRequiresSeparateValue = nameof(OptionRequiresSeparateValue);

    /// <summary>
    /// The option value is missing.
    /// </summary>
    public const string MissingOptionValue = nameof(MissingOptionValue);

    /// <summary>
    /// The scalar option was specified more than once.
    /// </summary>
    public const string RepeatedOption = nameof(RepeatedOption);

    /// <summary>
    /// A required option is missing.
    /// </summary>
    public const string MissingRequiredOption = nameof(MissingRequiredOption);

    /// <summary>
    /// A required argument is missing.
    /// </summary>
    public const string MissingRequiredArgument = nameof(MissingRequiredArgument);

    /// <summary>
    /// A positional value was unexpected.
    /// </summary>
    public const string UnexpectedArgument = nameof(UnexpectedArgument);

    /// <summary>
    /// Too few variadic values were supplied.
    /// </summary>
    public const string TooFewVariadicValues = nameof(TooFewVariadicValues);

    /// <summary>
    /// Too many variadic values were supplied.
    /// </summary>
    public const string TooManyVariadicValues = nameof(TooManyVariadicValues);

    /// <summary>
    /// A parameter value was invalid.
    /// </summary>
    public const string InvalidParameterValue = nameof(InvalidParameterValue);

    /// <summary>
    /// An unexpected error occurred.
    /// </summary>
    public const string UnexpectedError = nameof(UnexpectedError);
}
