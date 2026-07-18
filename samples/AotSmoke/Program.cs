using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R8.XunitLogger;

// Build a logger through the non-xunit entry point and emit one line. If the process exits 0 and prints
// the log line, the library survived Native AOT trimming.
var services = new ServiceCollection()
    .AddLogging()
    .AddXunitLogger(Console.WriteLine, options => options.MinimumLevel = LogLevel.Information)
    .BuildServiceProvider();

var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AotSmoke");
logger.LogInformation("AOT smoke: R8.XunitLogger works under Native AOT");

Console.WriteLine("AOT smoke OK");
return 0;
