# R8.XunitLogger

Bridges **Microsoft.Extensions.Logging** with xUnit's `ITestOutputHelper` so every `ILogger` call in your application appears in the test runner's output window — with no boilerplate.

Supports **xUnit v2** (`net6.0` / `netstandard2.1`) and **xUnit v3** (`net8.0` / `net10.0`).

[![NuGet version](https://img.shields.io/nuget/v/R8.XunitLogger)](https://www.nuget.org/packages/R8.XunitLogger/)
[![NuGet downloads](https://img.shields.io/nuget/dt/R8.XunitLogger)](https://www.nuget.org/packages/R8.XunitLogger/)
[![Last commit](https://img.shields.io/github/last-commit/iamr8/R8.XunitLogger)](https://github.com/iamr8/R8.XunitLogger/commits/master)

---

## Installation

```bash
dotnet add package R8.XunitLogger
```

---

## Options

| Option | Type | Default | Description |
|---|---|---|---|
| `MinimumLevel` | `LogLevel` | `Information` | Global minimum log level. Acts as the fallback when no `SetMinimumLevel` override or `IConfiguration` entry matches. |
| `IncludeTimestamp` | `bool` | `true` | Prepend `[date time]` to every log line. |
| `IncludeScopes` | `bool` | `false` | Render the active logging scope chain (`=> outer => inner`) before each message. |
| `ServiceProvider` | `IServiceProvider?` | `null` | When set, log-level filtering is read from `IConfiguration` (`Logging:LogLevel` section) instead of the options above. Automatically set to the DI container's `IServiceProvider` when using `AddXunitLogger`, but you can override it with any `IServiceProvider` of your choice — in both unit and integration tests. |
| `SetMinimumLevel(…)` | fluent chain | — | Per-namespace level overrides. See [Per-namespace levels](#per-namespace-log-levels). |
| ~~`Categories`~~ | `IList<string>` | — | **Obsolete.** Use `SetMinimumLevel` instead. |

---

## Usage

### Unit tests (`AddXunit`)

```csharp
public class UnitTest
{
    private readonly ILoggerFactory _loggerFactory;

    public UnitTest(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new LoggerFactory().AddXunit(outputHelper, options =>
        {
            options.MinimumLevel = LogLevel.Debug;
            options.IncludeScopes = true;
        });
    }

    [Fact]
    public void Test()
    {
        var logger = _loggerFactory.CreateLogger<MyService>();
        // ...
    }
}
```

### Integration tests with DI — per test (`AddXunitLogger`)

The test class itself implements `IXunitLogProvider`, so each test gets its own
`ITestOutputHelper` wired directly:

```csharp
public class IntegrationTest : IXunitLogProvider, IDisposable
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ServiceProvider _serviceProvider;

    public IntegrationTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddXunitLogger(message => OnWriteLine?.Invoke(message))
            .AddScoped<MyService>()
            .BuildServiceProvider();
        OnWriteLine += _outputHelper.WriteLine;
    }

    public event Action<string>? OnWriteLine;

    public void Dispose() => OnWriteLine -= _outputHelper.WriteLine;

    [Fact]
    public void Test()
    {
        var svc = _serviceProvider.GetRequiredService<MyService>();
        // ...
    }
}
```

### Integration tests with DI — shared fixture (`IClassFixture`)

Use this when the service container is expensive to build and should be shared
across all tests in the class. The fixture implements `IXunitLogProvider`; each
test subscribes its own `ITestOutputHelper` in the constructor and unsubscribes
in `Dispose`:

```csharp
// Fixture (shared across tests in the same class)
public class MyFixture : IXunitLogProvider
{
    public readonly ServiceProvider Services;

    public MyFixture()
    {
        Services = new ServiceCollection()
            .AddLogging()
            .AddXunitLogger(message => OnWriteLine?.Invoke(message), options =>
            {
                options.MinimumLevel = LogLevel.Warning;
            })
            .AddScoped<MyService>()
            .BuildServiceProvider();
    }

    public event Action<string>? OnWriteLine;
}

// Test class
public class MyTests : IClassFixture<MyFixture>, IDisposable
{
    private readonly MyFixture _fixture;
    private readonly ITestOutputHelper _outputHelper;

    public MyTests(MyFixture fixture, ITestOutputHelper outputHelper)
    {
        _fixture = fixture;
        _outputHelper = outputHelper;
        _fixture.OnWriteLine += _outputHelper.WriteLine;
    }

    public void Dispose() => _fixture.OnWriteLine -= _outputHelper.WriteLine;

    [Fact]
    public void Test()
    {
        var svc = _fixture.Services.GetRequiredService<MyService>();
        // ...
    }
}
```

### With `appsettings.json` / `IConfiguration`

Register `IConfiguration` in the DI container before calling `AddXunitLogger`.
`AddXunitLogger` automatically wires the service provider so `Logging:LogLevel`
entries from your configuration override `MinimumLevel` and `SetMinimumLevel`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MyApp.Services": "Debug"
    }
  }
}
```

```csharp
_serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build())
    .AddLogging()
    .AddXunitLogger(message => OnWriteLine?.Invoke(message))
    .BuildServiceProvider();
```

> **Note:** When using `AddXunitLogger`, `ServiceProvider` is automatically set to the DI container's own `IServiceProvider`. You can still override it with a custom one if needed.

---

## Per-namespace log levels

`SetMinimumLevel` returns a dedicated chain object so you can stack overrides
without accidentally accessing other options:

```csharp
options.MinimumLevel = LogLevel.Warning;   // global default

options
    .SetMinimumLevel("MyApp",         LogLevel.Debug)    // everything in MyApp → Debug
    .SetMinimumLevel("MyApp.Data",    LogLevel.Warning)  // but Data layer → Warning
    .SetMinimumLevel("Microsoft",     LogLevel.None)     // silence Microsoft internals
    .SetMinimumLevel("System",        LogLevel.None);
```

The **longest matching prefix** wins. `MinimumLevel` is used when no prefix matches.

---

## Scope rendering

Set `IncludeScopes = true` to render the active scope chain in Microsoft.Extensions.Logging style:

```
[25/04/2026 13:00:00] warn: MyApp.Services.OrderService[0]
      => Request abc-123 => ProcessOrder
      Insufficient stock for product 42
```

Supported scope types:

- **String**: `logger.BeginScope("my scope")` → `=> my scope`
- **Structured**: `logger.BeginScope("Order {Id}", 42)` → `=> Order 42`
- **Dictionary**: `logger.BeginScope(new Dictionary<string,object>{ ["OrderId"] = 42, ["CustomerId"] = "cust-99" })` → `=> OrderId: 42 => CustomerId: cust-99`
- **Nested**: outer → inner rendered left-to-right

Example:

```csharp
using (logger.BeginScope(new Dictionary<string, object>
       {
           ["OrderId"] = 42,
           ["CustomerId"] = "cust-99"
       }))
{
    logger.LogInformation("Processing order");
    logger.LogWarning("Stock low for order");
}
```

---

[See full examples in the sample project](https://github.com/iamr8/R8.XunitLogger/tree/master/R8.XunitLogger.Sample)
