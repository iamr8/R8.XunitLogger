# R8.XunitLogger
Seamlessly integrate Xunit's `ITestOutputHelper` with `Microsoft.Extensions.Logging` using `netstandard2.1`. Capture logs from any layer of your application and send them to your test output, making debugging and integration testing a breeze. With minimal configuration, you can monitor log messages, helping to ensure your application runs smoothly.

### Options
| Option                              | Description                                                                  | Default Value          | Xunit | Xunit Forwarding |
|-------------------------------------|------------------------------------------------------------------------------|------------------------|-------|------------------|
| MinLevel                            | The minimum level of log messages to be written to the test output           | `LogLevel.Information` | ✅     | ✅                |
| IncludeTimestamp                    | Whether to include timestamp in log messages                                 | `true`                 | ✅     | ✅                |
| [EnableColors](#colored-log-levels) | Whether to enable colored log levels                                         | `true`                 | ✅     | ✅                |
| Categories                          | The categories (namespaces) of log messages to be written to the test output | `null`                 | ✅     | ✅                |
| ServiceProvider                     | The service provider to be get `appsettings.json` from `IConfiguration`      | `null`                 | ❌     | ✅                |

### Colored Log Levels
To enable colored log levels, you need to set the following attribute to `true` in your `XunitLoggerOptions`/`XunitForwardingLoggerOptions`:
```csharp
public bool EnableColors { get; set; }
```
| Tested on                 | Supported | xUnit Version |
|---------------------------|-----------|---------------|
| Visual Studio 2022 17.6.2 | ❌         | 2.5.2         |
| Rider 2023.1.2            | ✅         | 2.5.2         |

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
            options.MinLevel = LogLevel.Debug;
            options.IncludeTimestamp = true;
        });
    }
}
```

For integration testing:
```csharp
public class IntegrationTest : IXunitForwardingLogProvider
{
    public IntegrationTest(ITestOutputHelper outputHelper)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitForwardingLoggerProvider(WriteLine)
            .BuildServiceProvider();
        this.OnWriteLine += outputHelper.WriteLine;
    }

    public event LogDelegate? OnWriteLine;
    public void WriteLine(string message) => OnWriteLine?.Invoke(message);
}
```

[See more examples here](https://github.com/iamr8/R8.XunitLogger/tree/master/R8.XunitLogger.Sample)

---

## Output
```text
[6/7/2023 12:19:07 AM] info: R8.XunitLogger.Sample.DummyObj
  This is an information message
[6/7/2023 12:19:07 AM] warn: R8.XunitLogger.Sample.DummyObj
  This is a warning message
[6/7/2023 12:19:07 AM] fail: R8.XunitLogger.Sample.DummyObj
  This is an error message
```