// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

public interface IAsyncCommand
{
    Task<int> ExecuteAsync(CommandContext context);
}
