namespace MattEland.DigitalDungeonMaster.ConsoleApp.Helpers;

public static class InputHelpers
{
    public static bool IsExitCommand(this string? input)
    {
        input = input?.Trim();

        return string.IsNullOrWhiteSpace(input)
             || input.Equals("exit", StringComparison.CurrentCultureIgnoreCase)
             || input.Equals("quit", StringComparison.CurrentCultureIgnoreCase)
             || input.Equals("goodbye", StringComparison.CurrentCultureIgnoreCase)
             || input.Equals("q", StringComparison.CurrentCultureIgnoreCase)
             || input.Equals("x", StringComparison.CurrentCultureIgnoreCase)
             || input.Equals("bye", StringComparison.CurrentCultureIgnoreCase);
    }
}