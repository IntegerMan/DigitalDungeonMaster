using Azure;
using Serilog.Core;

namespace MattEland.BasementsAndBasilisks.ConsoleApp;

public static class DisplayHelpers
{
    public static void SayDungeonMasterLine(ChatResult response, Logger logger)
    {
        if (response.FunctionsCalled.Any())
        {
            Console.WriteLine();
        }

        // List all function calls
        foreach (var call in response.FunctionsCalled)
        {
            AnsiConsole.MarkupLineInterpolated($"[Orange3]Called function[/]: [Yellow]{call}[/]");
        }

        SayDungeonMasterLine(response.Message, logger);
    }

    public static void SayDungeonMasterLine(string message, Logger logger)
    {
        logger.Information(message);
        Console.WriteLine();
        try
        {
            AnsiConsole.MarkupLine("[SteelBlue]DM[/]: " + message);
        }
        catch (Exception ex)
        {
            // Fallback to WriteLine in cases where response is not valid markup
            AnsiConsole.WriteLine("DM: " + message);

            logger.Error(ex, "An unhandled exception of type {Type} occurred in {Method}: {Message}", ex.GetType().FullName, nameof(SayDungeonMasterLine), ex.Message);
        }
    }
}