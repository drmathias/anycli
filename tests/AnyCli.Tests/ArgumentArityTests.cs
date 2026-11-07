// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli.Tests;

public class ArgumentArityTests
{
    [Test]
    public async Task Presets_ExposeExpectedRanges()
    {
        await Assert.That(ArgumentArity.ExactlyOne.Minimum).IsEqualTo(1);
        await Assert.That(ArgumentArity.ExactlyOne.Maximum).IsEqualTo(1);
        await Assert.That(ArgumentArity.ZeroOrOne.Minimum).IsEqualTo(0);
        await Assert.That(ArgumentArity.ZeroOrOne.Maximum).IsEqualTo(1);
        await Assert.That(ArgumentArity.ZeroOrMore.Minimum).IsEqualTo(0);
        await Assert.That(ArgumentArity.ZeroOrMore.Maximum).IsEqualTo(int.MaxValue);
        await Assert.That(ArgumentArity.OneOrMore.Minimum).IsEqualTo(1);
        await Assert.That(ArgumentArity.OneOrMore.Maximum).IsEqualTo(int.MaxValue);
    }

    [Test]
    public async Task IsRequiredAndIsVariadic_AreDerivedFromRange()
    {
        await Assert.That(ArgumentArity.ExactlyOne.IsRequired).IsTrue();
        await Assert.That(ArgumentArity.ExactlyOne.IsVariadic).IsFalse();
        await Assert.That(ArgumentArity.ZeroOrOne.IsRequired).IsFalse();
        await Assert.That(ArgumentArity.ZeroOrOne.IsVariadic).IsFalse();
        await Assert.That(ArgumentArity.ZeroOrMore.IsRequired).IsFalse();
        await Assert.That(ArgumentArity.ZeroOrMore.IsVariadic).IsTrue();
        await Assert.That(ArgumentArity.OneOrMore.IsRequired).IsTrue();
        await Assert.That(ArgumentArity.OneOrMore.IsVariadic).IsTrue();
    }

    [Test]
    [Arguments(-1, 1)]
    [Arguments(0, 0)]
    [Arguments(2, 1)]
    public async Task Constructor_RejectsInvalidRanges(int minimum, int maximum)
    {
        ArgumentOutOfRangeException? exception = null;

        try
        {
            _ = new ArgumentArity(minimum, maximum);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }

    [Test]
    public async Task AddVariadic_RejectsNonVariadicArity()
    {
        ArgumentException? exception = null;

        try
        {
            _ = new CommandArgumentBuilder().AddVariadic("tags", ArgumentArity.ExactlyOne);
        }
        catch (ArgumentException ex)
        {
            exception = ex;
        }

        await Assert.That(exception).IsNotNull();
    }
}
