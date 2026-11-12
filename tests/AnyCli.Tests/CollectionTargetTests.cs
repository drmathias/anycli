// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

using AnyCli.Tests.Fixtures;

[assembly: ParameterValueParser<List<string>, AnyCli.Tests.CommaSeparatedListParser>]

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
    public async Task BindsIList()
    {
        var app = CreateApp<IListCommand, IListParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,2,3");
    }

    [Test]
    public async Task BindsLinkedList()
    {
        var app = CreateApp<LinkedListCommand, LinkedListParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsReadOnlyCollection()
    {
        var app = CreateApp<ReadOnlyCollectionConcreteCommand, ReadOnlyCollectionConcreteParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsCollection()
    {
        var app = CreateApp<CollectionConcreteCommand, CollectionConcreteParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsImmutableArray()
    {
        var app = CreateApp<ImmutableArrayCommand, ImmutableArrayParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsImmutableList()
    {
        var app = CreateApp<ImmutableListCommand, ImmutableListParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsImmutableHashSet()
    {
        var app = CreateApp<ImmutableHashSetCommand, ImmutableHashSetParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task VariadicMaximumCount_AllowsValuesUpToMaximum()
    {
        var app = CreateApp<MaximumCountCommand, MaximumCountParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2"]);

        await Assert.That(result.Output).IsEqualTo("1,2");
    }

    [Test]
    public async Task VariadicMaximumCount_RejectsExcessValues()
    {
        var app = CreateApp<MaximumCountCommand, MaximumCountParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.ExitCode).IsEqualTo(2);
        await Assert.That(result.Output).IsEmpty();
        await Assert.That(result.Error).Contains("'values'");
        await Assert.That(result.Error).Contains("at most 2");
    }

    [Test]
    public async Task BindsIReadOnlySet()
    {
        var app = CreateApp<ReadOnlySetCommand, ReadOnlySetParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsISet()
    {
        var app = CreateApp<SetCommand, SetParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsFrozenSet()
    {
        var app = CreateApp<FrozenSetCommand, FrozenSetParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsIReadOnlyCollection()
    {
        var app = CreateApp<ReadOnlyCollectionCommand, ReadOnlyCollectionParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2", "3"]);

        await Assert.That(result.Output).IsEqualTo("1,2,3");
    }

    [Test]
    public async Task BindsNonVariadicCollectionWithPropertyParser()
    {
        var app = CreateApp<CustomParsedCollectionCommand, CustomParsedCollectionParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "a,b,c"]);

        await Assert.That(result.Output).IsEqualTo("a,b,c");
    }

    [Test]
    public async Task BindsNonVariadicCollectionWithRegisteredParser()
    {
        var app = CreateApp<RegisteredParsedCollectionCommand, RegisteredParsedCollectionParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values", "a,b,c"]);

        await Assert.That(result.Output).IsEqualTo("a,b,c");
    }

    [Test]
    public async Task BindsOptionalNonVariadicCollectionWithParser()
    {
        var app = CreateApp<OptionalParsedCollectionCommand, OptionalParsedCollectionParameters>("values");

        var result = await TestAppHarness.RunAsync(app, ["values"]);

        await Assert.That(result.Output).IsEqualTo("");
    }

    [Test]
    public async Task GeneratedOneOrMore_RequiresAtLeastOneValue()
    {
        var app = CreateApp<GeneratedOneOrMoreCommand, GeneratedOneOrMoreParameters>("values");

        var failure = await TestAppHarness.RunAsync(app, ["values"]);
        var result = await TestAppHarness.RunAsync(app, ["values", "1", "2"]);

        await Assert.That(failure.ExitCode).IsEqualTo(2);
        await Assert.That(failure.Output).IsEmpty();
        await Assert.That(failure.Error).Contains("'values'");
        await Assert.That(failure.Error).Contains("at least 1");
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

    private sealed class IListCommand : ICommand<IListParameters>
    {
        public int Execute(CommandContext context, IListParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class LinkedListCommand : ICommand<LinkedListParameters>
    {
        public int Execute(CommandContext context, LinkedListParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class ReadOnlyCollectionConcreteCommand : ICommand<ReadOnlyCollectionConcreteParameters>
    {
        public int Execute(CommandContext context, ReadOnlyCollectionConcreteParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class CollectionConcreteCommand : ICommand<CollectionConcreteParameters>
    {
        public int Execute(CommandContext context, CollectionConcreteParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class ImmutableArrayCommand : ICommand<ImmutableArrayParameters>
    {
        public int Execute(CommandContext context, ImmutableArrayParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class ImmutableListCommand : ICommand<ImmutableListParameters>
    {
        public int Execute(CommandContext context, ImmutableListParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class ImmutableHashSetCommand : ICommand<ImmutableHashSetParameters>
    {
        public int Execute(CommandContext context, ImmutableHashSetParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values.Order()));
            return 0;
        }
    }

    private sealed class MaximumCountCommand : ICommand<MaximumCountParameters>
    {
        public int Execute(CommandContext context, MaximumCountParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class ReadOnlySetCommand : ICommand<ReadOnlySetParameters>
    {
        public int Execute(CommandContext context, ReadOnlySetParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values.Order()));
            return 0;
        }
    }

    private sealed class SetCommand : ICommand<SetParameters>
    {
        public int Execute(CommandContext context, SetParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values.Order()));
            return 0;
        }
    }

    private sealed class FrozenSetCommand : ICommand<FrozenSetParameters>
    {
        public int Execute(CommandContext context, FrozenSetParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values.Order()));
            return 0;
        }
    }

    private sealed class ReadOnlyCollectionCommand : ICommand<ReadOnlyCollectionParameters>
    {
        public int Execute(CommandContext context, ReadOnlyCollectionParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class CustomParsedCollectionCommand : ICommand<CustomParsedCollectionParameters>
    {
        public int Execute(CommandContext context, CustomParsedCollectionParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class RegisteredParsedCollectionCommand : ICommand<RegisteredParsedCollectionParameters>
    {
        public int Execute(CommandContext context, RegisteredParsedCollectionParameters parameters)
        {
            context.Console.Output.Write(string.Join(",", parameters.Values));
            return 0;
        }
    }

    private sealed class OptionalParsedCollectionCommand : ICommand<OptionalParsedCollectionParameters>
    {
        public int Execute(CommandContext context, OptionalParsedCollectionParameters parameters)
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

public sealed partial class IListParameters
{
    [VariadicArgument(0)]
    public IList<int> Values { get; set; } = [];
}

public sealed partial class LinkedListParameters
{
    [VariadicArgument(0)]
    public LinkedList<int> Values { get; set; } = [];
}

public sealed partial class ReadOnlyCollectionConcreteParameters
{
    [VariadicArgument(0)]
    public ReadOnlyCollection<int> Values { get; set; } = Array.AsReadOnly(Array.Empty<int>());
}

public sealed partial class CollectionConcreteParameters
{
    [VariadicArgument(0)]
    public Collection<int> Values { get; set; } = new();
}

public sealed partial class ImmutableArrayParameters
{
    [VariadicArgument(0)]
    public ImmutableArray<int> Values { get; set; } = ImmutableArray<int>.Empty;
}

public sealed partial class ImmutableListParameters
{
    [VariadicArgument(0)]
    public ImmutableList<int> Values { get; set; } = ImmutableList<int>.Empty;
}

public sealed partial class ImmutableHashSetParameters
{
    [VariadicArgument(0)]
    public ImmutableHashSet<int> Values { get; set; } = ImmutableHashSet<int>.Empty;
}

public sealed partial class MaximumCountParameters
{
    [VariadicArgument(0, MaximumCount = 2)]
    public int[] Values { get; set; } = [];
}

public sealed partial class ReadOnlySetParameters
{
    [VariadicArgument(0)]
    public IReadOnlySet<int> Values { get; set; } = new HashSet<int>();
}

public sealed partial class SetParameters
{
    [VariadicArgument(0)]
    public ISet<int> Values { get; set; } = new HashSet<int>();
}

public sealed partial class FrozenSetParameters
{
    [VariadicArgument(0)]
    public FrozenSet<int> Values { get; set; } = FrozenSet<int>.Empty;
}

public sealed partial class ReadOnlyCollectionParameters
{
    [VariadicArgument(0)]
    public IReadOnlyCollection<int> Values { get; set; } = [];
}

public sealed partial class CustomParsedCollectionParameters
{
    [Argument(0, Parser = typeof(CommaSeparatedListParser))]
    public List<string> Values { get; set; } = [];
}

public sealed partial class RegisteredParsedCollectionParameters
{
    [Argument(0)]
    public List<string> Values { get; set; } = [];
}

public sealed partial class OptionalParsedCollectionParameters
{
    [OptionalArgument(0, Parser = typeof(CommaSeparatedListParser))]
    public List<string> Values { get; set; } = [];
}

public sealed class CommaSeparatedListParser : IParameterParser<List<string>>
{
    public static bool TryParse(
        string value,
        IFormatProvider? provider,
        out List<string> result)
    {
        result = [.. value.Split(',')];
        return true;
    }
}
