// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Provides the data shared by root and command help output.
/// </summary>
public abstract class HelpContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HelpContext"/> class.
    /// </summary>
    /// <param name="appName">The application name.</param>
    /// <param name="console">The console writer.</param>
    /// <param name="globalOptions">The application-wide options.</param>
    protected HelpContext(
        string appName,
        IConsoleWriter console,
        IReadOnlyList<OptionInfo> globalOptions)
    {
        AppName = appName;
        Console = console;
        GlobalOptions = globalOptions;
    }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public string AppName { get; }

    /// <summary>
    /// Gets the console writer used for help output.
    /// </summary>
    public IConsoleWriter Console { get; }

    /// <summary>
    /// Gets the application-wide options, such as <c>--help</c> and <c>--version</c>.
    /// </summary>
    public IReadOnlyList<OptionInfo> GlobalOptions { get; }
}
