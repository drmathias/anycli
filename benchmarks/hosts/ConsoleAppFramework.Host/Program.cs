// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using ConsoleAppFramework;

var app = ConsoleApp.Create();
app.Add("run", ([Argument] string input, int count = 0, bool verbose = false) => { });
app.Run(args);
return 0;
