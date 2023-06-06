using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    internal static class LogProviderHelper
    {
        public static LogLevel GetMinimumLevel(IServiceProvider serviceProvider, string categoryName, LogLevel defaultMinLevel, IEnumerable<string> categories)
        {
            if (serviceProvider.GetService(typeof(IConfiguration)) is IConfiguration configuration)
            {
                var providers = configuration
                    .GetSection("Logging:LogLevel")
                    .GetChildren()
                    .ToDictionary(x => x.Key, x => Enum.Parse<LogLevel>(x.Value));
                if (!providers.Any())
                    return defaultMinLevel;

                var pairs = providers.Where(logCategory => categoryName.StartsWith(logCategory.Key)).ToArray();
                if (pairs.Length > 0)
                {
                    var defaultLogLevel = pairs.Length == 1 ? pairs.First() : pairs.MaxBy(c => c.Key.Length);
                    return defaultLogLevel.Value;
                }
                else
                {
                    return providers["Default"];
                }
            }
            else
            {
                var c = categories.TryGetNonEnumeratedCount(out var cc) ? cc : categories.Count();
                if (c == 0)
                    return defaultMinLevel;
            
                var array = categories.Where(categoryName.StartsWith).ToArray();
                if (array.Length > 0)
                {
                    return defaultMinLevel;
                }
                else
                {
                    return LogLevel.None;
                }
            }
        }

        public static string FormatLog<TState>(string categoryName, bool includeTimestamp, LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string> formatter, bool colorize)
        {
            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            using var ms = new MemoryStream();
            using TextWriter tw = new StreamWriter(ms, Encoding.UTF8, -1, true);

            if (includeTimestamp)
            {
                tw.Write("[");
                tw.Write(DateTime.Now.ToString("G", CultureInfo.CurrentCulture));
                tw.Write("] ");
            }

            tw.WriteConsole(logLevel switch
            {
                LogLevel.Trace => "trce",
                LogLevel.Debug => "dbug",
                LogLevel.Information => "info",
                LogLevel.Warning => "warn",
                LogLevel.Error => "fail",
                LogLevel.Critical => "crit",
            }, logLevel switch
            {
                LogLevel.Trace => ConsoleColor.White,
                LogLevel.Debug => ConsoleColor.White,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.White,
                LogLevel.Error => ConsoleColor.White,
                LogLevel.Critical => ConsoleColor.White,
                _ => ConsoleColor.Gray
            }, logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Black,
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Information => ConsoleColor.Green,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.Gray
            }, colorize);
            tw.Write(": ");
            tw.Write(categoryName);
            tw.Write(Environment.NewLine);
            tw.Write("      ");
            tw.Write(message);
            if (exception != null)
            {
                tw.Write(Environment.NewLine);
                tw.Write(exception.ToString());
            }

            tw.Flush();
            ms.TryGetBuffer(out var buffer);
            var log = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            return log;
        }
    }
}