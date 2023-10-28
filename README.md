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
[See examples](https://github.com/iamr8/R8.XunitLogger/tree/master/R8.XunitLogger.Sample)

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

---
## Conclusion
This package is a simple implementation of `ILoggerProvider` and `ILogger` interfaces, which is useful for Xunit tests. It's not a replacement for `ILoggerProvider` and `ILogger` interfaces, and it's not a replacement for `ILoggerFactory` and `ILogger<T>` interfaces. It's just a simple implementation of these interfaces, which is useful for Xunit tests.
