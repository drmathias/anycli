using System.Globalization;

namespace AnyCli;

public interface ICommand<TExitCode> : ICommand
    where TExitCode : struct, Enum
{
    new TExitCode Execute(CommandContext context);

    int ICommand.Execute(CommandContext context) => Convert.ToInt32(Execute(context), CultureInfo.InvariantCulture);
}
