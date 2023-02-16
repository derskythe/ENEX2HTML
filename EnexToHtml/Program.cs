using Microsoft.Extensions.Logging;

namespace EnexToHtml;

/// <summary>
/// Class Program.
/// </summary>
public static class Program
{
    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length is < 1 or > 2)
            {
                EnexParser.WriteMessage("Invalid number of arguments", LogLevel.Error);
                EnexParser.WriteMessage("Usage:\nenextohtml.exe <Directory with files or file> [<Stored directory>]",
                                        LogLevel.Information,
                                        false);
                return;
            }

            var count = EnexParser.Parse(args[0],
                                         args.Length > 1 ? args[1] : string.Empty);

            EnexParser.WriteMessage($"Parsed: {count} file(s)",
                                    LogLevel.Information);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Thread.Sleep(1500);
            throw;
        }
    }
}