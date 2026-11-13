// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Errors;

internal static class CommandErrorResourceKeys
{
    public const string NoCommandSpecified = nameof(NoCommandSpecified);
    public const string UnknownCommand = nameof(UnknownCommand);
    public const string MissingSubcommand = nameof(MissingSubcommand);
    public const string UnknownOption = nameof(UnknownOption);
    public const string MalformedOption = nameof(MalformedOption);
    public const string OptionRequiresSeparateValue = nameof(OptionRequiresSeparateValue);
    public const string MissingOptionValue = nameof(MissingOptionValue);
    public const string RepeatedOption = nameof(RepeatedOption);
    public const string MissingRequiredOption = nameof(MissingRequiredOption);
    public const string MissingRequiredArgument = nameof(MissingRequiredArgument);
    public const string UnexpectedArgument = nameof(UnexpectedArgument);
    public const string TooFewVariadicValues = nameof(TooFewVariadicValues);
    public const string TooManyVariadicValues = nameof(TooManyVariadicValues);
    public const string InvalidParameterValue = nameof(InvalidParameterValue);
}
