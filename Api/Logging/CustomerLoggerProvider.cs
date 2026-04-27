using System.Collections.Concurrent;

namespace SistemaProdutos.Logging
{
    public class CustomerLoggerProvider : ILoggerProvider
    {
        readonly CustomerLoggerProviderConfiguration loggerConfig;
        readonly ConcurrentDictionary<String, CustomerLogger> loggers =
            new ConcurrentDictionary<string, CustomerLogger>();

        public CustomerLoggerProvider (CustomerLoggerProviderConfiguration config)
        {
            loggerConfig = config;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => CustomerLogger(name, loggerConfig));
        }

        private CustomerLogger CustomerLogger(string name, CustomerLoggerProviderConfiguration loggerConfig)
        {
            return null;
        }

        public void Dispose()
        {
            loggers.Clear();
        }

        
    }
}
