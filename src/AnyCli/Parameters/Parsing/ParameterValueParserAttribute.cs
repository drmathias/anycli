// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Registers a compile-time parameter parser for a value type.
/// </summary>
/// <typeparam name="TValue">The value type parsed by <typeparamref name="TParser"/>.</typeparam>
/// <typeparam name="TParser">The parser type.</typeparam>
/// <remarks>
/// Apply this attribute at the assembly level. The source generator uses the registration to
/// emit a direct call to <typeparamref name="TParser"/> for matching command arguments and options.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class ParameterValueParserAttribute<TValue, TParser> : Attribute
    where TParser : IParameterParser<TValue>
{
}
