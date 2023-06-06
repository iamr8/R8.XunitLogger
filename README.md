# XunitLogger
A low-config Log Provider for Xunit tests to add more resolution on services.

```csharp
// For simple usage
var loggerFactory = new LoggerFactory().AddXunit(outputHelper, options => options.MinLevel = LogLevel.Debug);

// For integration tests
// It is better to inherit from `IFixtureLogProvider` interface, to keep the methods in a single place
public class IntegrationTestFixture : IFixtureLogProvider
{
    public IntegrationTestFixture(ITestOutputHelper outputHelper)
    {
        this._serviceProvider = new ServiceCollection()
            .AddLogging()
            // Add the following line to your service collection
            .AddXunitForwardingLoggerProvider(WriteLine, options => options.MinLevel = LogLevel.Warning)
            .AddScoped<DummyObj>()
            .BuildServiceProvider();
        this.OnWriteLine += outputHelper.WriteLine;
    }

    public event LogDelegate? OnWriteLine;
    public void WriteLine(string message) => OnWriteLine?.Invoke(message);
}
```
*If you're encountering a mixed output of logger, you need to inherit `IDisposable` interface, and remove the event handler, on the same way you added it.*
```csharp
public void Dispose()
{
    this._fixture.OnWriteLine -= outputHelper.WriteLine;
}
```

### Options
According to that the strategies to get logs in Unit Tests and Integration Tests are different, you have two different options to configure the logger:
- `XunitLoggerOptions` for Unit Tests
- `XunitForwardingLoggerOptions` for Integration Tests

### Colored Log Levels
To enable colored log levels, you need to set the following attribute to `true` in your `XunitLoggerOptions`/`XunitForwardingLoggerOptions`:
```csharp
public bool EnableColors { get; set; }
```
| IDE                       | Supported | xUnit Version |
|---------------------------|-----------|---------------|
| Visual Studio 2022 17.6.2 | ❌       | 2.4.2         |
| Rider 2023.1.2            | ✅       | 2.4.2         |

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

---

## Conclusion
This package is a simple implementation of `ILoggerProvider` and `ILogger` interfaces, which is useful for Xunit tests. It's not a replacement for `ILoggerProvider` and `ILogger` interfaces, and it's not a replacement for `ILoggerFactory` and `ILogger<T>` interfaces. It's just a simple implementation of these interfaces, which is useful for Xunit tests.

**#Women_Life_Freedom** :v: