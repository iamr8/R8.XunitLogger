using System;
using System.IO;

namespace R8.XunitLogger
{
    internal static class TextWriterHelper
    {
        private const string DefaultForegroundColor = "\x1B[39m\x1B[22m";
        private const string DefaultBackgroundColor = "\x1B[49m";

        private static bool IsRider
        {
            get
            {
                var host = Environment.GetEnvironmentVariable("RESHARPER_HOST");
                return !string.IsNullOrEmpty(host) && host.Equals("Rider", StringComparison.Ordinal);
            }
        }

        public static void WriteConsole(this TextWriter textWriter, string message, ConsoleColor? background, ConsoleColor? foreground, LoggerColorBehavior colorBehavior)
        {
            LoggerColorBehavior lcb;
            if (colorBehavior == LoggerColorBehavior.Default)
                lcb = IsRider ? LoggerColorBehavior.Enabled : LoggerColorBehavior.Disabled;
            else
                lcb = colorBehavior;
            
            if (lcb == LoggerColorBehavior.Enabled)
            {
                var backgroundColor = background.HasValue ? GetBackgroundColorEscapeCode(background.Value) : null;
                var foregroundColor = foreground.HasValue ? GetForegroundColorEscapeCode(foreground.Value) : null;

                if (backgroundColor != null)
                {
                    textWriter.Write(backgroundColor);
                }

                if (foregroundColor != null)
                {
                    textWriter.Write(foregroundColor);
                }

                textWriter.Write(message);

                if (foregroundColor != null)
                {
                    textWriter.Write(DefaultForegroundColor);
                }

                if (backgroundColor != null)
                {
                    textWriter.Write(DefaultBackgroundColor);
                }
            }
            else
            {
                textWriter.Write(message);
            }
        }

        private static string GetForegroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\x1B[30m",
                ConsoleColor.DarkRed => "\x1B[31m",
                ConsoleColor.DarkGreen => "\x1B[32m",
                ConsoleColor.DarkYellow => "\x1B[33m",
                ConsoleColor.DarkBlue => "\x1B[34m",
                ConsoleColor.DarkMagenta => "\x1B[35m",
                ConsoleColor.DarkCyan => "\x1B[36m",
                ConsoleColor.Gray => "\x1B[37m",
                ConsoleColor.Red => "\x1B[1m\x1B[31m",
                ConsoleColor.Green => "\x1B[1m\x1B[32m",
                ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
                ConsoleColor.Blue => "\x1B[1m\x1B[34m",
                ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
                ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
                ConsoleColor.White => "\x1B[1m\x1B[37m",

                _ => DefaultForegroundColor
            };

        private static string GetBackgroundColorEscapeCode(ConsoleColor color) =>
            color switch
            {
                ConsoleColor.Black => "\x1B[40m",
                ConsoleColor.DarkRed => "\x1B[41m",
                ConsoleColor.DarkGreen => "\x1B[42m",
                ConsoleColor.DarkYellow => "\x1B[43m",
                ConsoleColor.DarkBlue => "\x1B[44m",
                ConsoleColor.DarkMagenta => "\x1B[45m",
                ConsoleColor.DarkCyan => "\x1B[46m",
                ConsoleColor.Gray => "\x1B[47m",

                _ => DefaultBackgroundColor
            };
    }
}