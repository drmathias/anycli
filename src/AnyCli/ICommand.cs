namespace AnyCli;

public interface ICommand
{
    int Execute(CommandContext context);
}
