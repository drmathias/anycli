// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Help;

/// <summary>
/// Provides the data shared by root and command help output.
/// </summary>
public abstract class HelpContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HelpContext"/> class.
    /// </summary>
    /// <param name="console">The console writer.</param>
    /// <param name="application">The application metadata.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="console"/> or <paramref name="application"/> is <see langword="null"/>.
    /// </exception>
    protected HelpContext(
        IConsoleWriter console,
        ApplicationInfo application)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(application);

        Console = console;
        Application = application;
    }

    /// <summary>
    /// Gets the application metadata.
    /// </summary>
    public ApplicationInfo Application { get; }

    /// <summary>
    /// Gets the console writer used for help output.
    /// </summary>
    public IConsoleWriter Console { get; }
}
