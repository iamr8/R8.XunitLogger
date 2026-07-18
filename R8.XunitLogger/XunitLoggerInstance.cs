using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
#if NET8_0_OR_GREATER
using Xunit;
#else
using Xunit.Abstractions;
#endif

namespace R8.XunitLogger
{
    internal class XunitLoggerInstance : ILogger
    {
        private readonly string _categoryName;
        private readonly Action<string>? _onWriteLine;
        private readonly ITestOutputHelper? _output;
        private readonly XunitLoggerOptions _options;
        private readonly LogLevel _minLevel;

        private const string Padding = "      ";

        public XunitLoggerInstance(string categoryName, IServiceProvider? serviceProvider, Action<string>? onWriteLine, ITestOutputHelper? output, XunitLoggerOptions options)
        {
            _categoryName = categoryName;
            _onWriteLine = onWriteLine;
            _output = output;
            _options = options;
#pragma warning disable CS0618 // Categories is obsolete — consumed internally with full awareness
            _minLevel = GetMinimumLevel(serviceProvider, _categoryName, options.MinimumLevel, options.Categories, options.Overrides);
#pragma warning restore CS0618
        }

        // Explicit interface implementation avoids CS8633 (nullability constraint mismatch
        // between our implementation and the ILogger interface definition across TFMs).
        IDisposable ILogger.BeginScope<TState>(TState state)
        {
#pragma warning disable CA1510
            if (state == null)
                throw new ArgumentNullException(nameof(state));
#pragma warning restore CA1510

            return XunitLoggerScope.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var line = BuildLogLine(logLevel, eventId, formatter(state, exception), exception);

            try
            {
                if (_output != null)
                    _output.WriteLine(line);
                else
                    _onWriteLine?.Invoke(line);
            }
            catch (InvalidOperationException ex)
            {
                // Ignore exception if the application tries to log after the test ends
                // but before the ITestOutputHelper is detached, e.g. "There is no currently active test."
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Formats a single log entry into the final output string.
        /// Extracted to keep <see cref="Log{TState}"/> under the method-length threshold.
        /// </summary>
        private string BuildLogLine(LogLevel logLevel, EventId eventId, string message, Exception? exception)
        {
            // Use a StringWriter backed by an internal StringBuilder to avoid the
            // MemoryStream → byte-encoding → string round-trip of the previous implementation.
            using var tw = new StringWriter();

            if (_options.IncludeTimestamp)
            {
                tw.Write('[');
                tw.Write(DateTime.Now.ToString("G", CultureInfo.CurrentCulture));
                tw.Write("] ");
            }

            tw.WriteConsole(
                GetLevelLabel(logLevel),
                ConsoleColor.White,   // background (all levels use white text)
                GetLevelColor(logLevel));

            tw.Write(": ");
            tw.Write(_categoryName);
            tw.Write('[');
            tw.Write(eventId);
            tw.Write(']');
            tw.WriteLine();

            if (_options.IncludeScopes)
                GetScopeInformation(tw);

            tw.Write(Padding);
            tw.Write(message);

            if (exception != null)
            {
                tw.WriteLine();
                tw.Write(exception.ToString());
            }

            return tw.ToString();
        }

        private static string GetLevelLabel(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => "none"
        };

        private static ConsoleColor GetLevelColor(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => ConsoleColor.Black,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Information => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => ConsoleColor.Gray
        };

        private static LogLevel GetMinimumLevel(
            IServiceProvider? serviceProvider,
            string categoryName,
            LogLevel defaultMinLevel,
            IList<string> categories,
            IReadOnlyDictionary<string, LogLevel> overrides)
        {
            if (serviceProvider != null && serviceProvider.GetService(typeof(IConfiguration)) is IConfiguration configuration)
            {
                var providers = configuration
                    .GetSection("Logging:LogLevel")
                    .GetChildren()
                    .ToDictionary(x => x.Key, x => Enum.Parse<LogLevel>(x.Value!), StringComparer.Ordinal);

                if (providers.Count == 0)
                    return defaultMinLevel;

                var pairs = providers
                    .Where(logCategory => categoryName.StartsWith(logCategory.Key, StringComparison.Ordinal))
                    .ToArray();

                if (pairs.Length > 0)
                {
                    // Pick the most specific (longest) matching prefix.
                    var best = pairs.Length == 1
                        ? pairs[0]
                        : pairs.OrderByDescending(p => p.Key.Length).First();
                    return best.Value;
                }

                // Fall back to the "Default" key; if absent, return defaultMinLevel.
                return providers.TryGetValue("Default", out var fallback) ? fallback : defaultMinLevel;
            }
            else
            {
                // 1. SetMinimumLevel overrides – per-namespace override, most specific prefix wins.
                if (overrides.Count > 0)
                {
                    var matched = overrides
                        .Where(kv => categoryName.StartsWith(kv.Key, StringComparison.Ordinal))
                        .OrderByDescending(kv => kv.Key.Length)
                        .Cast<KeyValuePair<string, LogLevel>?>()
                        .FirstOrDefault();

                    if (matched.HasValue)
                        return matched.Value.Value;
                }

                // 2. Categories – legacy simple whitelist; unmatched categories are silenced.
#pragma warning disable CS0618
                if (categories.Count > 0)
                {
                    var matched = categories.Any(c => categoryName.StartsWith(c, StringComparison.Ordinal));
                    return matched ? defaultMinLevel : LogLevel.None;
                }
#pragma warning restore CS0618

                // 3. No filter configured – use the global minimum level.
                return defaultMinLevel;
            }
        }

        /// <summary>
        /// Writes the current scope chain to <paramref name="textWriter" /> in Microsoft.Extensions.Logging style:
        /// <code>      => outerScope => innerScope</code>
        /// Matches the format produced by <c>SimpleConsoleFormatter</c> when <c>IncludeScopes = true</c>.
        /// </summary>
        private static void GetScopeInformation(TextWriter textWriter)
        {
            var current = XunitLoggerScope.Current;
            if (current == null)
                return;

            // Walk from innermost → outermost, pushing onto a stack so we can
            // pop in outermost → innermost order (matches MS console formatter behaviour).
            var stack = new Stack<XunitLoggerScope>();
            while (current != null)
            {
                stack.Push(current);
                current = current.Parent;
            }

            // Microsoft style: "      => scope1 => scope2 => scope3\n"
            // First entry gets the padding + "=> "; subsequent entries get " => " separator.
            var isFirst = true;
            while (stack.Count > 0)
            {
                foreach (var text in GetScopes(stack.Pop()))
                {
                    if (isFirst)
                    {
                        textWriter.Write(Padding);
                        textWriter.Write("=> ");
                        isFirst = false;
                    }
                    else
                    {
                        textWriter.Write(" => ");
                    }

                    textWriter.Write(text);
                }
            }

            // Only write the trailing newline if at least one scope was rendered.
            if (!isFirst)
                textWriter.WriteLine();
        }

        /// <summary>
        /// Returns one or more stringified properties from the log scope.
        /// </summary>
        private static IEnumerable<string> GetScopes(XunitLoggerScope scope)
        {
            if (scope.State is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                // Microsoft.Extensions.Logging.FormattedLogValues implements
                // IEnumerable<KVP> but its ToString() gives the final formatted string.
                var type = scope.State.GetType();
                if (string.Equals(type.FullName, "Microsoft.Extensions.Logging.FormattedLogValues", StringComparison.Ordinal))
                {
                    yield return scope.State.ToString()!;
                }
                else
                {
                    foreach (var pair in pairs)
                        yield return $"{pair.Key}: {pair.Value}";
                }
            }
            else if (scope.State is IEnumerable<string> entries)
            {
                foreach (var entry in entries)
                    yield return entry;
            }
            else
            {
                yield return scope.State.ToString()!;
            }
        }
    }
}

