// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Configures application-wide defaults for command argument parsing.
/// </summary>
public sealed class CommandParsingDefaults
{
    internal IEnumParser EnumParser { get; private set; } =
        EnumParserAdapter<DefaultEnumArgumentParser>.Instance;

    /// <summary>
    /// Overrides the parser used for all enum arguments.
    /// </summary>
    /// <typeparam name="TParser">The enum parser type.</typeparam>
    /// <returns>The current defaults instance.</returns>
    public CommandParsingDefaults SetEnumArgumentParser<TParser>()
        where TParser : IEnumArgumentParser
    {
        EnumParser = EnumParserAdapter<TParser>.Instance;
        return this;
    }
}
