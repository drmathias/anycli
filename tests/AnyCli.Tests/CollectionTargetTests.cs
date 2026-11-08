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
        var app = CreateApp<ReadOnlyListCommand, ReadOnlyListParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsList()
    {
        var app = CreateApp<ListCommand, ListParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsICollection()
    {
        var app = CreateApp<CollectionCommand, CollectionParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsIEnumerable()
    {
        var app = CreateApp<EnumerableCommand, EnumerableParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsHashSet()
    {
        var app = CreateApp<HashSetCommand, HashSetParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task GeneratedOneOrMore_RequiresAtLeastOneValue()
    {
        var app = CreateApp<GeneratedOneOrMoreCommand, GeneratedOneOrMoreParameters>("values");
        CommandParameterException? exception = null;

        try
        {
            _ = await TestAppHarness.RunAsync(app, ["values"]);
        }
        catch (CommandParameterException ex)
        {
            exception = ex;
        }

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2"]);

        await Assert.That(exception).IsNotNull();
        await Assert.That(exception!.Message).Contains("'values'");
        await Assert.That(result.Output).IsEqualTo("1,2");
    }

    private static ConsoleApp CreateApp<TCommand, TParameters>(string name)
        where TCommand : class, ICommand<TParameters>
        where TParameters : class, ICommandParameters<TParameters> =>
        ConsoleAppBuilder.Create()
            .WithName("anycli")
            .ConfigureCommands(commands => commands.AddCommand<TCommand, TParameters>(name))
            .Build();

    private sealed class ReadOnlyListCommand : ICommand<ReadOnlyListParameters>
    {
        public int Execute(CommandContext context, ReadOnlyListParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class ListCommand : ICommand<ListParameters>
    {
        public int Execute(CommandContext context, ListParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class CollectionCommand : ICommand<CollectionParameters>
    {
        public int Execute(CommandContext context, CollectionParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class EnumerableCommand : ICommand<EnumerableParameters>
    {
        public int Execute(CommandContext context, EnumerableParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class HashSetCommand : ICommand<HashSetParameters>
    {
        public int Execute(CommandContext context, HashSetParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values.Order()));
            return 0;
        }
    }

    private sealed class GeneratedOneOrMoreCommand : ICommand<GeneratedOneOrMoreParameters>
    {
        public int Execute(CommandContext context, GeneratedOneOrMoreParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }
}

public sealed partial class ReadOnlyListParameters
{
    [VariadicArgument(0)]
    public IReadOnlyList<int> Values { get; set; } = [];
}

public sealed partial class ListParameters
{
    [VariadicArgument(0)]
    public List<int> Values { get; set; } = [];
}

public sealed partial class CollectionParameters
{
    [VariadicArgument(0)]
    public ICollection<int> Values { get; set; } = [];
}

public sealed partial class EnumerableParameters
{
    [VariadicArgument(0)]
    public IEnumerable<int> Values { get; set; } = [];
}

public sealed partial class HashSetParameters
{
    [VariadicArgument(0)]
    public HashSet<int> Values { get; set; } = [];
}

public sealed partial class GeneratedOneOrMoreParameters
{
    [VariadicArgument(0, MinimumCount = 1)]
    public int[] Values { get; set; } = [];
}
