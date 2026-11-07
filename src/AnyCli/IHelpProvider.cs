// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

public interface IHelpProvider
{
    void WriteHelp(HelpContext context);
}
