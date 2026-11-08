// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests.Fixtures;

internal sealed record TestAppResult(int ExitCode, string Output, string Error);
