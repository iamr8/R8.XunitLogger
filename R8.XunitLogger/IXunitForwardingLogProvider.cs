using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace R8.XunitLogger
{
    /// <summary>
    /// An interface to bring <see cref="IXunitForwardingLogProvider"/> to test fixtures.
    /// </summary>
    public interface IXunitForwardingLogProvider
    {
        /// <summary>
        /// An event to register/unregister <see cref="ITestOutputHelper.WriteLine(string)"/> to be used by <see cref="XunitForwardingLoggerExtensions.AddXunitForwardingLoggerProvider"/>.
        /// </summary>
        /// <remarks>Please note that it's recommended to Unregister the event in <see cref="System.IDisposable.Dispose"/> method to avoid being consumed by other tests.</remarks>
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