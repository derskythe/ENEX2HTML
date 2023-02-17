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

    public class TagResource
    {
        public string Mime { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Hash { get; set; }
        public string FileName { get; set; }

        public TagResource(string mime, string fileName, string sourceUrl)
        {
            Mime = mime;
            Hash = SetHash(sourceUrl);
            FileName = fileName;
        }

        public TagResource(string mime, int width, int height, string fileName, string sourceUrl)
        {
            Mime = mime;
            Width = width;
            Height = height;
            Hash = SetHash(sourceUrl);
            FileName = fileName;
        }

        private static string SetHash(string sourceUrl)
        {
            // en-cache://tokenKey%3D%22AuthToken%3AUser%3A2966622%22+
            // 68868e8f-ec7b-432e-9eb1-6f8c1804988c+
            // 06b3ceb9f8432939881cea18b8df2659+
            // https://www.evernote.com/shard/s26/res/d41fbdc0-682f-45bd-92b4-2dfc5297f31a
            var splited = sourceUrl.Split('+');
            return splited[2];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"FileName: {FileName}, Mime: {Mime},  Hash: {Hash}, Width: {Width}, Height: {Height}";
        }
    }
}