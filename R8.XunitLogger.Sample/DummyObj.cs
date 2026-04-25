using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace R8.XunitLogger.Sample
{
    public class DummyObj
    {
        private readonly ILogger<DummyObj> _logger;

        public DummyObj(ILogger<DummyObj> logger)
        {
            _logger = logger;
        }

        public void Test()
        {
            _logger.LogDebug("This is a debug message");
            _logger.LogInformation("This is an information message");
            _logger.LogWarning("This is a warning message");
            _logger.LogError("This is an error message");
        }

        public void Test2()
        {
            using (_logger.BeginScope("Test Scope"))
            {
                _logger.LogDebug("This is a debug message");
                _logger.LogInformation("This is an information message");
                _logger.LogWarning("This is a warning message");
                _logger.LogError("This is an error message");
            }
        }
        
        public void Test3()
        {
            using (_logger.BeginScope("Scope {ScopeId}", "Root"))
            {
                _logger.LogDebug("This is a debug message");
                _logger.LogInformation("This is an information message");
                using (_logger.BeginScope("Scope {ScopeId}. L {FabricatedNum}", "Nested", 1))
                {
                    _logger.LogWarning("This is a warning message");
                    _logger.LogError("This is an error message");
                }
            }
        }

        public void Test4()
        {
            using (_logger.BeginScope(new Dictionary<string, object>
                   {
                       ["OrderId"] = 42,
                       ["CustomerId"] = "cust-99"
                   }))
            {
                _logger.LogInformation("Processing order");
                _logger.LogWarning("Stock low for order");
            }
        }
    }
}