// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
// SPDX-License-Identifier: MIT

namespace AnyCli;

/// <summary>
/// Describes how many values a positional argument accepts.
/// </summary>
public readonly record struct ArgumentArity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentArity"/> struct.
    /// </summary>
    /// <param name="minimum">The minimum number of values. Must be non-negative.</param>
    /// <param name="maximum">
    /// The maximum number of values. Must be at least one and at least
    /// <paramref name="minimum"/>. Use <see cref="int.MaxValue"/> for no upper bound.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="minimum"/> is negative, <paramref name="maximum"/> is less than one,
    /// or <paramref name="minimum"/> is greater than <paramref name="maximum"/>.
    /// </exception>
    public ArgumentArity(int minimum, int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minimum);

        if (maximum >= 1)
        {
            if (minimum > maximum)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimum),
                    "The minimum value count cannot exceed the maximum value count.");
            }

            Minimum = minimum;
            Maximum = maximum;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(maximum));
        }
    }

    /// <summary>
    /// Gets the minimum number of values.
    /// </summary>
    public int Minimum { get; }

    /// <summary>
    /// Gets the maximum number of values, or <see cref="int.MaxValue"/> for no upper bound.
    /// </summary>
    public int Maximum { get; }

    /// <summary>
    /// Gets a value indicating whether at least one value is required.
    /// </summary>
    public bool IsRequired => Minimum > 0;

    /// <summary>
    /// Gets a value indicating whether the argument accepts more than one value.
    /// </summary>
    public bool IsVariadic => Maximum > 1;

    /// <summary>
    /// Gets an arity that accepts exactly one value.
    /// </summary>
    public static ArgumentArity ExactlyOne { get; } = new(1, 1);

    /// <summary>
    /// Gets an arity that accepts zero or one value.
    /// </summary>
    public static ArgumentArity ZeroOrOne { get; } = new(0, 1);

    /// <summary>
    /// Gets an arity that accepts zero or more values.
    /// </summary>
    public static ArgumentArity ZeroOrMore { get; } = new(0, int.MaxValue);

    /// <summary>
    /// Gets an arity that accepts one or more values.
    /// </summary>
    public static ArgumentArity OneOrMore { get; } = new(1, int.MaxValue);
}
