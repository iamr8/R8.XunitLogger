# XunitLogger
Seamlessly integrate Xunit's `ITestOutputHelper` with `Microsoft.Extensions.Logging` using `netstandard2.1`. Capture logs from any layer of your application and send them to your test output, making debugging and integration testing a breeze. With minimal configuration, you can monitor log messages, helping to ensure your application runs smoothly.

### Options
| Option                              | Description                                                                  | Default Value          | Xunit | Xunit Forwarding |
|-------------------------------------|------------------------------------------------------------------------------|------------------------|-------|------------------|
| MinLevel                            | The minimum level of log messages to be written to the test output           | `LogLevel.Information` | ✅     | ✅                |
| IncludeTimestamp                    | Whether to include timestamp in log messages                                 | `true`                 | ✅     | ✅                |
| [EnableColors](#colored_log_levels) | Whether to enable colored log levels                                         | `true`                 | ✅     | ✅                |
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
In the following, you can find some usage examples of this package. However, the full definition of these examples are exist in the [Sample](R8.XunitLogger.Sample) folder.

### Unit Tests
```csharp
using R8.XunitLogger;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class UnitTest
{
    private readonly ILoggerFactory _loggerFactory;

    public UnitTest(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new LoggerFactory().AddXunit(outputHelper, options =>
        {
            options.MinLevel = LogLevel.Debug; // Default is `LogLevel.Information`
            options.IncludeTimestamp = true; // Default is `true`
            options.EnableColors = true; // Default is `true`
            options.Categories = new[] { "R8.XunitLogger.Sample.DummyObj" }; // Optional
        });
    }

    [Fact]
    public void Test1()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<DummyObj>();
        var dummy = new DummyObj(logger);
        
        // Act
        dummy.Test();
        
        // Assert
        Assert.True(true);
    }
}
```

---

### Integration Tests

####
```csharp
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

public class IntegrationTest : IFixtureLogProvider, IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

    public IntegrationTest(ITestOutputHelper outputHelper)
    {
        this._serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitForwardingLoggerProvider(WriteLine, options => 
            {
                options.MinLevel = LogLevel.Information; // Default is `LogLevel.Information`
                options.IncludeTimestamp = true; // Default is `true`
            })
            .AddScoped<DummyObj>()
            .BuildServiceProvider();
        this.OnWriteLine += outputHelper.WriteLine;
    }

    public event LogDelegate? OnWriteLine;
    public void WriteLine(string message) => OnWriteLine?.Invoke(message);

    public void Dispose()
    {
        // It's recommended to remove the event handler, to avoid mixing logs from different tests
        this.OnWriteLine -= OnWriteLine;
    }

    [Fact]
    public void Test1()
    {
        // Act
        Dummy.Test();

        // Assert
        Assert.True(true);
    }
}
```
#### with `IClassFixture`:
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class IntegrationTestClassFixture : IClassFixture<IntegrationTestFixture>, IDisposable
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _outputHelper;

    public IntegrationTestClassFixture(IntegrationTestFixture fixture, ITestOutputHelper outputHelper)
    {
        this._fixture = fixture;
        _outputHelper = outputHelper;
        this._fixture.OnWriteLine += _outputHelper.WriteLine;
    }

    public void Dispose()
    {
        // It's recommended to remove the event handler, to avoid mixing logs from different tests
        this._fixture.OnWriteLine -= _outputHelper.WriteLine;
    }

    [Fact]
    public void Test1()
    {
        // Act
        this._fixture.Dummy.Test();

        // Assert
        Assert.True(true);
    }
}

public class IntegrationTestFixture : IFixtureLogProvider
{
    private readonly ServiceProvider _serviceProvider;

    protected internal DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

    public IntegrationTestFixture()
    {
        this._serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitForwardingLoggerProvider(WriteLine, options => options.MinLevel = LogLevel.Warning)
            .AddScoped<DummyObj>()
            .BuildServiceProvider();
    }

    public event LogDelegate? OnWriteLine;
    public void WriteLine(string message) => OnWriteLine?.Invoke(message);
}
```

#### or with a `IConfiguration` (`appsettings.json`)
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

public class IntegrationTestWithConfigurations : IFixtureLogProvider, IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    private DummyObj Dummy => _serviceProvider.GetRequiredService<DummyObj>();

    public IntegrationTestWithConfigurations(ITestOutputHelper outputHelper)
    {
        this._serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(sp => new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .Build())
            .AddLogging()
            .AddXunitForwardingLoggerProvider(WriteLine, options => options.MinLevel = LogLevel.Debug)
            .AddScoped<DummyObj>()
            .BuildServiceProvider();
        this.OnWriteLine += outputHelper.WriteLine;
    }

    public event LogDelegate? OnWriteLine;
    public void WriteLine(string message) => OnWriteLine?.Invoke(message);

    public void Dispose()
    {
        // It's recommended to remove the event handler, to avoid mixing logs from different tests
        this.OnWriteLine -= OnWriteLine;
    }

    [Fact]
    public void Test1()
    {
        // Act
        Dummy.Test();

        // Assert
        Assert.True(true);
    }
}
```

### Output
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

**#Women_Life_Freedom** :v:
