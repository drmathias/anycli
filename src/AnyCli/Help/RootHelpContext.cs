// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides the data needed to render root help output.
/// </summary>
public sealed class RootHelpContext : HelpContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootHelpContext"/> class.
    /// </summary>
    /// <param name="console">The console writer.</param>
    /// <param name="application">The application metadata.</param>
    public RootHelpContext(
        IConsoleWriter console,
        ApplicationInfo application)
        : base(console, application)
    {
    }
}
