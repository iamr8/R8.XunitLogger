using System;

namespace R8.XunitLogger
{
    /// <summary>
    /// Implemented by test fixtures that want to forward xUnit test output calls into the logging pipeline.
    /// Implement this interface on your fixture class, subscribe <see cref="OnWriteLine" />
    /// to the xUnit output helper's <c>WriteLine</c> in the test constructor,
    /// and unsubscribe in <see cref="System.IDisposable.Dispose" />.
    /// </summary>
    public interface IXunitLogProvider
    {
        /// <summary>
        /// Raised for every log line produced by the xUnit logger. Subscribe this to
        /// the xUnit output helper's <c>WriteLine</c> method so that log output
        /// appears in the test runner's output window.
        /// </summary>
        /// <remarks>Unregister in <see cref="System.IDisposable.Dispose" /> to avoid leaking across tests.</remarks>
#pragma warning disable MA0046 // Event uses Action<string> intentionally; changing to EventHandler<T> would be a breaking change.
        event Action<string> OnWriteLine;
#pragma warning restore MA0046
    }
}