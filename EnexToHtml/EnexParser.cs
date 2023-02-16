using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace EnexToHtml;

public class EnexParser
{
    public static int Parse(string path, string storedPath = "")
    {
        var processedFiles = 0;
        if (Path.EndsInDirectorySeparator(path))
        {
            // Process directory
            if (!Directory.Exists(path))
            {
                WriteMessage("Path doesn't exists, exit", LogLevel.Error);
                return processedFiles;
            }

            if (string.IsNullOrWhiteSpace(storedPath))
            {
                storedPath = path;
            }
            else if (!Directory.Exists(storedPath))
            {
                WriteMessage("Invalid path for storing, exit", LogLevel.Error);
                return processedFiles;
            }

            var files = Directory.EnumerateFiles(path, "*.enec").ToArray();

            if (files.Length == 0)
            {
                WriteMessage("Can't find ENEC files in path", LogLevel.Error);
                return processedFiles;
            }

            foreach (var file in files)
            {
                ParseSingleFile(file, storedPath);
                processedFiles++;
            }
        }
        else
        {
            // Process file
            if (!File.Exists(path))
            {
                WriteMessage("File doesn't exists, exit", LogLevel.Error);
                return processedFiles;
            }

            ParseSingleFile(path);
            processedFiles++;
        }

        return processedFiles;
    }

    private static void ParseSingleFile(string file, string storedPath = "")
    {
        var info = new FileInfo(file);
        WriteMessage($"Starting parse file: {info.Name}{info.Extension}", LogLevel.Information);

        if (string.IsNullOrEmpty(storedPath))
        {
            storedPath = info.DirectoryName ?? throw new ArgumentNullException(nameof(file));
        }
        using var inputStream = info.OpenRead();
        inputStream.Close();

        Debug.Assert(!string.IsNullOrEmpty(storedPath));
        WriteMessage($"Write HTML file: {info.Name}.html", LogLevel.Information);
    }

    public static void WriteMessage(string message, LogLevel level, bool withDateTime = true)
    {
        var color = Console.ForegroundColor;
        var str = new StringBuilder();
        if (withDateTime)
        {
            str.Append('[').Append(DateTime.Now.ToString("T")).Append("] - ");
        }

        str.Append(message);
        switch (level)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(str.ToString());
                Console.ForegroundColor = color;
                break;
            case LogLevel.None:
            case LogLevel.Information:
                Console.WriteLine(message);
                break;
            case LogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(str.ToString());
                Console.ForegroundColor = color;
                break;
            case LogLevel.Error:
            case LogLevel.Critical:
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(str.ToString());
                Console.ForegroundColor = color;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }
}