using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

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
            _minLevel = serviceProvider != null
                ? GetMinimumLevel(serviceProvider, _categoryName, options.MinimumLevel, options.Categories)
                : options.MinimumLevel;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return XunitLoggerScope.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter == null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception);

            using var ms = new MemoryStream();
            using TextWriter tw = new StreamWriter(ms, Encoding.UTF8, -1, true);

            if (_options.IncludeTimestamp)
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
            }, _options.ColorBehavior);
            tw.Write(": ");

            tw.Write(_categoryName);

            tw.Write('[');
            tw.Write(eventId);
            tw.Write(']');
            tw.WriteLine();

            if (_options.IncludeScopes)
            {
                GetScopeInformation(tw);
            }

            tw.Write(Padding);
            tw.Write(message);
            if (exception != null)
            {
                tw.WriteLine();
                tw.Write(exception.ToString());
            }

            tw.Flush();
            if (!ms.TryGetBuffer(out var buffer) || buffer.Array == null)
                return;

            var line = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);

            try
            {
                if (_output != null)
                    _output.WriteLine(line);
                else
                    _onWriteLine?.Invoke(line);
            }
            catch (InvalidOperationException)
            {
                // Ignore exception if the application tries to log after the test ends
                // but before the ITestOutputHelper is detached, e.g. "There is no currently active test."
            }
        }

        private static LogLevel GetMinimumLevel(IServiceProvider serviceProvider, string categoryName, LogLevel defaultMinLevel, IList<string> categories)
        {
            if (serviceProvider.GetService(typeof(IConfiguration)) is IConfiguration configuration)
            {
                var providers = configuration
                    .GetSection("Logging:LogLevel")
                    .GetChildren()
                    .ToDictionary(x => x.Key, x => Enum.Parse<LogLevel>(x.Value), StringComparer.Ordinal);
                if (providers.Count == 0)
                    return defaultMinLevel;

                var pairs = providers
                    .Where(logCategory => categoryName.StartsWith(logCategory.Key, StringComparison.Ordinal))
                    .ToArray();
                if (pairs.Length > 0)
                {
                    var defaultLogLevel = pairs.Length == 1
                        ? pairs[0]
                        : pairs.Select(pair => new
                            {
                                Pair = pair,
                                Length = pair.Key.Length,
                            })
                            .OrderByDescending(x => x.Length)
                            .First().Pair;
                    return defaultLogLevel.Value;
                }
                else
                {
                    return providers["Default"];
                }
            }
            else
            {
                if (categories.Count == 0)
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


        /// <summary>
        /// Gets the scope information for the current operation.
        /// </summary>
        /// <param name="textWriter">The <see cref="StringBuilder"/> to write the scope to.</param>
        private static void GetScopeInformation(TextWriter textWriter)
        {
            var current = XunitLoggerScope.Current;

            var stack = new Stack<XunitLoggerScope>();
            while (current != null)
            {
                stack.Push(current);
                current = current.Parent;
            }

            var index = 0;
            var lastIndex = stack.Count - 1;
            while (stack.Count > 0)
            {
                if (index == 0) textWriter.Write(Padding);

                var elem = stack.Pop();
                foreach (var property in GetScopes(elem))
                {
                    if (index > 0) textWriter.Write(' ');
                    textWriter.Write("=> ");
                    textWriter.Write(property);
                    if (index == lastIndex) textWriter.WriteLine();
                    index++;
                }
            }
        }

        /// <summary>
        /// Returns one or more stringified properties from the log scope.
        /// </summary>
        /// <param name="scope">The <see cref="XunitLoggerScope"/> to stringify.</param>
        /// <returns>An enumeration of scope properties from the current scope.</returns>
        private static IEnumerable<string> GetScopes(XunitLoggerScope scope)
        {
            if (scope.State is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                var type = scope.State.GetType();
                if (string.Equals(type.FullName, "Microsoft.Extensions.Logging.FormattedLogValues", StringComparison.Ordinal))
                {
                    var values = scope.State.ToString();
                    yield return values;
                }
                else
                {
                    foreach (var pair in pairs)
                    {
                        yield return $"{pair.Key}: {pair.Value}";
                    }
                }
            }
            else if (scope.State is IEnumerable<string> entries)
            {
                foreach (var entry in entries)
                {
                    yield return entry;
                }
            }
            // else if (scope.State is FormattedLogValues formatted)
            // {
            //     foreach (var pair in formatted)
            //     {
            //         yield return $"{pair.Key}: {pair.Value}";
            //     }
            // }
            else
            {
                yield return scope.ToString();
            }
        }
    }
}