namespace AnyCli;

public interface IAsyncCommand
{
    Task<int> ExecuteAsync(CommandContext context);
}
