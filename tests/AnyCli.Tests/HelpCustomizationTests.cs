// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Globalization;

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class HelpCustomizationTests
{
    [Test]
    public async Task UseTextProvider_Instance_UsesProvidedInstance()
    {
        var textProvider = new CustomHelpTextProvider();

        var app = ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureHelp(help => help.UseTextProvider(textProvider))
            .ConfigureCommands(_ => { })
            .Build();

        var result = await TestAppHarness.RunAsync(app, ["--help"]);

        await Assert.That(result.ExitCode).IsEqualTo(0);
        await Assert.That(result.Output).Contains("Custom usage:");
        await Assert.That(result.Output).Contains("Custom options:");
        await Assert.That(result.Error).IsEmpty();
    }

    private sealed class CustomHelpTextProvider : IHelpTextProvider
    {
        public string GetUsageHeading() => "Custom usage";

        public string GetDescriptionHeading() => "Custom description";

        public string GetCommandsHeading() => "Custom commands";

        public string GetArgumentsHeading() => "Custom arguments";

        public string GetOptionsHeading() => "Custom options";

        public string GetOptionsPlaceholder() => "OPTIONS";

        public string GetCommandPlaceholder() => "COMMAND";

        public string GetDescription(HelpDescriptionContext context) =>
            context.Description.ToString();

        public string GetDefaultValueLabel() => "default";

        public string GetDefaultValueText(HelpText text) => text.ToString();

        public CultureInfo GetFormatCulture() => CultureInfo.InvariantCulture;
    }
}
