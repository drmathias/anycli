namespace AnyCli;

public sealed class CommandContext
{
    internal CommandContext(string name, TextWriter output, TextWriter error)
    {
        Name = name;
        Output = output;
        Error = error;
    }

    public string Name { get; }

    public TextWriter Output { get; }

    public TextWriter Error { get; }
}
