# R8.XunitLogger

Seamlessly integrate Xunit's `ITestOutputHelper` with `Microsoft.Extensions.Logging` using `netstandard2.1`, `net6.0`, `net7.0` and `net8.0`. Capture logs from any layer of your application and send them to your test output, making debugging and integration testing a breeze. With minimal configuration, you can monitor log messages, helping to ensure your application runs smoothly.

[![Nuget](https://img.shields.io/nuget/vpre/R8.XunitLogger)](https://www.nuget.org/packages/R8.XunitLogger/) ![Nuget](https://img.shields.io/nuget/dt/R8.XunitLogger) ![Commit](https://img.shields.io/github/last-commit/iamr8/R8.XunitLogger)

### Installation

```bash
dotnet add package R8.XunitLogger
```

### Options

| Option                               | Description                                                                  | Default Value                 |
|--------------------------------------|------------------------------------------------------------------------------|-------------------------------|
| MinimumLevel                         | The minimum level of log messages to be written to the test output           | `LogLevel.Information`        |
| IncludeTimestamp                     | Whether to include timestamp in log messages                                 | `true`                        |
| IncludeScopes                        | Whether to include scopes in log messages                                    | `false`                       |
| [ColorBehavior](#colored-log-levels) | Whether to enable colored log levels                                         | `LoggerColorBehavior.Default` |
| Categories                           | The categories (namespaces) of log messages to be written to the test output | `null`                        |
| ServiceProvider                      | The service provider to be get `appsettings.json` from `IConfiguration`      | `null`                        |

---

### Colored Log Levels

| Tested on                 | Minimum Supported Version | xUnit Version |
|---------------------------|---------------------------|---------------|
| Visual Studio 2022 17.6.2 | `Not supported`           | 2.5.2         |
| Rider 2023.3.2            | `Nearly all versions`     | 2.5.2         |

---

## Usage

For unit testing:

```csharp
public class UnitTest
{
    public UnitTest(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new LoggerFactory().AddXunit(outputHelper, options =>
        {
            // ... options
        });
    }
}
```

For integration testing:

```csharp
public class IntegrationTest : IXunitLogProvider
{
    public IntegrationTest(ITestOutputHelper outputHelper)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitForwardingLoggerProvider(message => OnWriteLine?.Invoke(message), options => 
            {
                // ... options
            })
            .BuildServiceProvider();
        this.OnWriteLine += outputHelper.WriteLine;
    }

    public event Action<string> OnWriteLine;
}
```

[See more examples here](https://github.com/iamr8/R8.XunitLogger/tree/master/R8.XunitLogger.Sample)