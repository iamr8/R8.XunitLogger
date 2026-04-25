using System;
using System.IO;

namespace R8.XunitLogger
{
    internal static class TextWriterHelper
    {
        private const string DefaultForegroundColor = "\x1B[39m\x1B[22m";
        private const string DefaultBackgroundColor = "\x1B[49m";

        // Cached once at startup – environment variables don't change during a process lifetime.
        private static readonly bool _isRider =
            string.Equals(Environment.GetEnvironmentVariable("RESHARPER_HOST"), "Rider", StringComparison.Ordinal);

        /// <summary>
        /// Writes <paramref name="message" /> to <paramref name="textWriter" />, optionally wrapping it
        /// with ANSI color escape codes.
        /// </summary>
        /// <remarks>
        /// ANSI escape codes are emitted only when running inside JetBrains Rider
        /// (detected via <c>RESHARPER_HOST=Rider</c>), which is the only test host whose
        /// output panel renders ANSI codes for both xUnit v2 and v3.
        /// Visual Studio and VS Code test output panels strip ANSI sequences and display
        /// them as raw text, so no codes are emitted there.
        /// </remarks>
        /// <param name="textWriter">The target writer.</param>
        /// <param name="message">The text to write.</param>
        /// <param name="background">Optional background <see cref="ConsoleColor" />.</param>
        /// <param name="foreground">Optional foreground <see cref="ConsoleColor" />.</param>
        public static void WriteConsole(this TextWriter textWriter, string message, ConsoleColor? background, ConsoleColor? foreground)
        {
            // Rider's output panel renders ANSI codes correctly for both xUnit v2 and xUnit v3.
            // Neither Visual Studio nor VS Code test output panels support ANSI, so we
            // always fall back to plain text outside Rider.
            if (_isRider)
            {
                var backgroundColor = background.HasValue ? GetBackgroundColorEscapeCode(background.Value) : null;
                var foregroundColor = foreground.HasValue ? GetForegroundColorEscapeCode(foreground.Value) : null;

                if (backgroundColor != null)
                    textWriter.Write(backgroundColor);

                if (foregroundColor != null)
                    textWriter.Write(foregroundColor);

                textWriter.Write(message);

                if (foregroundColor != null)
                    textWriter.Write(DefaultForegroundColor);

                if (backgroundColor != null)
                    textWriter.Write(DefaultBackgroundColor);
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