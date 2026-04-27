namespace SistemaProdutos.Logging
{
    public class CustomerLogger : ILogger
    {
        readonly string? loggerName;
        readonly CustomerLoggerProviderConfiguration? loggerConfig;

        public CustomerLogger(string name, CustomerLoggerProviderConfiguration config)
        {
            loggerName = name;
            loggerConfig = config;
        }

        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            return null;
        }

        bool ILogger.IsEnabled(LogLevel logLevel)
        {
            return logLevel == loggerConfig?.LogLevel;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string mensagem = $"{logLevel.ToString()} {eventId.Id} - {formatter (state, exception)}";
            EscreverTextoNoArquivo(mensagem);
        }
        void EscreverTextoNoArquivo (string mensagem)
        {
            string caminhoArquivoLog = @"C:\Users\231.918058\Downloads\Ester.txt";

            using (StreamWriter streamWriter = new StreamWriter (caminhoArquivoLog, true))
            {
                try
                {
                    streamWriter.WriteLine(mensagem);
                    streamWriter.Close();
                }

                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
