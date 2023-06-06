using Microsoft.Extensions.Logging;

namespace R8.XunitLogger
{
    public interface IFixtureLogProvider
    {
        /// <summary>
        /// An event to be invoked when a log message is logged to be used in <see cref="XunitForwardingLoggerExtensions.AddXunitForwardingLoggerProvider"/>'s "onLog" parameter.
        /// </summary>
        /// <example>OnWriteLine += outputHelper.WriteLine</example>
        event LogDelegate OnWriteLine;
    
        /// <summary>
        /// A delegate to be invoked when a log message is logged to be used in <see cref="XunitForwardingLoggerExtensions.AddXunitForwardingLoggerProvider"/>'s "onLog" parameter.
        /// </summary>
        /// <example>
        /// public void WriteLine(string message) => OnWriteLine?.Invoke(message);
        /// </example>
        /// <param name="message">A log message that comes from <see cref="ILogger{TCategoryName}.Log{TState}"/> method.</param>
        void WriteLine(string message);
    }
}