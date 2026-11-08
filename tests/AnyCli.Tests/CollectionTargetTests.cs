// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using AnyCli.Tests.Fixtures;

namespace AnyCli.Tests;

public class CollectionTargetTests
{
    [Test]
    public async Task BindsIReadOnlyList()
    {
        var app = CreateApp<ReadOnlyListCommand, ReadOnlyListSettings>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsList()
    {
        var app = CreateApp<ListCommand, ListSettings>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsICollection()
    {
        var app = CreateApp<CollectionCommand, CollectionSettings>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsIEnumerable()
    {
        var app = CreateApp<EnumerableCommand, EnumerableSettings>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsHashSet()
    {
        var app = CreateApp<HashSetCommand, HashSetSettings>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task GeneratedOneOrMore_RequiresAtLeastOneValue()
    {
        var app = CreateApp<GeneratedOneOrMoreCommand, GeneratedOneOrMoreSettings>("values");
        CommandArgumentException? exception = null;

        try
        {
            _ = await TestAppHarness.RunAsync(app, ["values"]);
        }
        catch (CommandArgumentException ex)
        {
            exception = ex;
        }

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2"]);

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'values'");
        await Assert.That(result.Output).IsEqualTo("1,2");
    }

    private static ConsoleApp CreateApp<TCommand, TSettings>(string name)
        where TCommand : class, ICommand<TSettings>
        where TSettings : class, ICommandSettings<TSettings> =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TSettings>(name))
            .Build();

    private sealed class ReadOnlyListCommand : ICommand<ReadOnlyListSettings>
    {
        public int Execute(CommandContext context, ReadOnlyListSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Values));
            return 0;
        }
    }

    private sealed class ListCommand : ICommand<ListSettings>
    {
        public int Execute(CommandContext context, ListSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Values));
            return 0;
        }
    }

    private sealed class CollectionCommand : ICommand<CollectionSettings>
    {
        public int Execute(CommandContext context, CollectionSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Values));
            return 0;
        }
    }

    private sealed class EnumerableCommand : ICommand<EnumerableSettings>
    {
        public int Execute(CommandContext context, EnumerableSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Values));
            return 0;
        }
    }

    private sealed class HashSetCommand : ICommand<HashSetSettings>
    {
        public int Execute(CommandContext context, HashSetSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Values.Order()));
            return 0;
        }
    }

    private sealed class GeneratedOneOrMoreCommand : ICommand<GeneratedOneOrMoreSettings>
    {
        public int Execute(CommandContext context, GeneratedOneOrMoreSettings settings)
        {
            context.Console.Output.Write(string.Join(",", settings.Values));
            return 0;
        }
    }
}

public sealed partial class ReadOnlyListSettings
{
    [VariadicArgument(0)]
    public IReadOnlyList<int> Values { get; set; } = [];
}

public sealed partial class ListSettings
{
    [VariadicArgument(0)]
    public List<int> Values { get; set; } = [];
}

public sealed partial class CollectionSettings
{
    [VariadicArgument(0)]
    public ICollection<int> Values { get; set; } = [];
}

public sealed partial class EnumerableSettings
{
    [VariadicArgument(0)]
    public IEnumerable<int> Values { get; set; } = [];
}

public sealed partial class HashSetSettings
{
    [VariadicArgument(0)]
    public HashSet<int> Values { get; set; } = [];
}

public sealed partial class GeneratedOneOrMoreSettings
{
    [VariadicArgument(0, MinimumCount = 1)]
    public int[] Values { get; set; } = [];
}
