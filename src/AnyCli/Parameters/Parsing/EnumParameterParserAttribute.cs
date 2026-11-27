// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Parameters.Parsing;

/// <summary>
/// Registers a compile-time parser for all enum parameters.
/// </summary>
/// <typeparam name="TParser">The enum parser type.</typeparam>
/// <remarks>
/// Apply this attribute at the assembly level. The source generator uses the registration to
/// emit direct calls to <typeparamref name="TParser"/> for enum parameters. The parser may
/// implement <see cref="IEnumParameterParser"/> or <see cref="IAsyncEnumParameterParser"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class EnumParameterParserAttribute<TParser> : Attribute
{
}
