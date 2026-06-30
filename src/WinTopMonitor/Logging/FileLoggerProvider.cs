using Microsoft.Extensions.Logging;

namespace WinTopMonitor.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFile;
    private readonly object _lock = new();
    private bool _disposed;

    public FileLoggerProvider(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        _logFile = Path.Combine(logDirectory, $"wintop-{DateTime.UtcNow:yyyyMMdd}.jsonl");
    }

    public ILogger CreateLogger(string categoryName) => new FileLogger(categoryName, _logFile, _lock);

    public void Dispose()
    {
        _disposed = true;
    }

    private sealed class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logFile;
        private readonly object _lock;

        public FileLogger(string categoryName, string logFile, object syncLock)
        {
            _categoryName = categoryName;
            _logFile = logFile;
            _lock = syncLock;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var entry = new
            {
                timestamp = DateTimeOffset.UtcNow,
                level = logLevel.ToString(),
                category = _categoryName,
                eventId = eventId.Id,
                message = formatter(state, exception),
                exception = exception?.GetType().Name
            };

            var line = System.Text.Json.JsonSerializer.Serialize(entry);
            lock (_lock)
            {
                File.AppendAllText(_logFile, line + Environment.NewLine);
            }
        }
    }
}

