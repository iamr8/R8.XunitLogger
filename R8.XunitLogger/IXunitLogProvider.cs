using System;

using Xunit.Abstractions;

namespace R8.XunitLogger
{
    /// <summary>
    /// An interface to bring <see cref="IXunitLogProvider"/> to test fixtures.
    /// </summary>
    public interface IXunitLogProvider
    {
        /// <summary>
        /// An event to register/unregister <see cref="ITestOutputHelper.WriteLine(string)"/>.
        /// </summary>
        /// <remarks>Please note that it's recommended to Unregister the event in <see cref="System.IDisposable.Dispose"/> method to avoid being consumed by other tests.</remarks>
        event Action<string> OnWriteLine;
    }
}