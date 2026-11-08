// SPDX-FileCopyrightText: 2026 Adam Shirt <adamshirt@outlook.com>
//
// SPDX-License-Identifier: MIT

namespace AnyCli;

internal sealed class CommandNode(string name, string description)
{
    private readonly List<CommandNode> _children = [];
    private readonly Dictionary<string, CommandNode> _childrenByName = new(StringComparer.Ordinal);

    public string Name { get; } = name;

    public string Description { get; } = description;

    public IReadOnlyList<CommandNode> Children => _children;

    public CommandRegistration? Command { get; set; }

    public void AddChild(CommandNode child)
    {
        if (_childrenByName.ContainsKey(child.Name))
        {
            throw new InvalidOperationException(
                $"Command '{child.Name}' is already registered in this command group.");
        }

        _childrenByName[child.Name] = child;
        _children.Add(child);
    }

    public bool TryGetChild(string name, out CommandNode child) =>
        _childrenByName.TryGetValue(name, out child!);
}
