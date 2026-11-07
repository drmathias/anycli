// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// A command that executes synchronously with typed settings.
/// </summary>
/// <typeparam name="TSettings">The settings type.</typeparam>
public interface ICommand<TSettings>
    where TSettings : class
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The parsed command settings.</param>
    /// <returns>The process exit code.</returns>
    int Execute(CommandContext context, TSettings settings);
}

/// <summary>
/// A command that executes synchronously with typed settings and returns a custom exit code enum.
/// </summary>
/// <typeparam name="TSettings">The settings type.</typeparam>
/// <typeparam name="TExitCode">The enum type used as the exit code.</typeparam>
public interface ICommand<TSettings, TExitCode>
    where TSettings : class
    where TExitCode : struct, Enum
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="settings">The parsed command settings.</param>
    /// <returns>The command's exit code.</returns>
    TExitCode Execute(CommandContext context, TSettings settings);
}
