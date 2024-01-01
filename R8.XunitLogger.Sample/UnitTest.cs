using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace R8.XunitLogger.Sample
{
    public class UnitTest
    {
        private readonly ILoggerFactory _loggerFactory;

        public UnitTest(ITestOutputHelper outputHelper)
        {
            _loggerFactory = new LoggerFactory().AddXunit(outputHelper, options =>
            {
                options.MinLevel = LogLevel.Debug;
                options.ColorBehavior = LoggerColorBehavior.Disabled;
                options.IncludeScopes = true;
                options.Categories = new[] { "R8.XunitLogger.Sample.DummyObj" };
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
        
        [Fact]
        public void TestWithScope()
        {
            // Arrange
            var logger = _loggerFactory.CreateLogger<DummyObj>();
            var dummy = new DummyObj(logger);
            
            // Act
            dummy.Test2();
            
            // Assert
            Assert.True(true);
        }
        
        [Fact]
        public void TestWithNestedScopes()
        {
            // Arrange
            var logger = _loggerFactory.CreateLogger<DummyObj>();
            var dummy = new DummyObj(logger);
            
            // Act
            dummy.Test3();
            
            // Assert
            Assert.True(true);
        }
    }
}