using System.Text;

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

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            // Verifica se o nível de log está habilitado na configuração
            return loggerConfig != null && logLevel >= loggerConfig.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel) || formatter == null) return;

            string mensagem = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {eventId.Id} - {formatter(state, exception)}";

            if (exception != null)
            {
                mensagem += $" | Exception: {exception.Message} | StackTrace: {exception.StackTrace}";
            }

            EscreverTextoNoArquivo(mensagem);
        }

        private void EscreverTextoNoArquivo(string mensagem)
        {

            string caminhoArquivoLog = @"C:\Users\231.918058\Downloads\Ester.txt";

            try
            {

                using (StreamWriter streamWriter = new StreamWriter(caminhoArquivoLog, true, Encoding.UTF8))
                {
                    streamWriter.WriteLine(mensagem);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"FALHA NO LOGGER: {ex.Message}");
            }
        }
    }
}
