using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Logging
{
    /// <summary>
    /// Memory logger with limited size.  Normally used to see the last n number of log messages with test cases.
    /// </summary>
    public class MemoryLogger : ILogger
    {
        private readonly int _maxSize = 100;
        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        /// <summary>
        /// Construct with max size or default
        /// </summary>
        /// <param name="maxSize"></param>
        public MemoryLogger(string name, int maxSize = 100)
        {
            Name = name;
            _maxSize = maxSize;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _queue.Enqueue($"{Name}: " + formatter(state, exception));

            while (_queue.Count > _maxSize) _queue.TryDequeue(out string _);
        }

        public IReadOnlyList<string> LoggedItems => _queue.ToArray();

        /// <summary>
        /// Name of logger
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Create a type logger for memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ILogger<T> CreateLogger<T>() => new MemoryLoggerStub<T>();

        private class MemoryLoggerStub<T> : MemoryLogger, ILogger<T>
        {
            public MemoryLoggerStub() : base(typeof(T).Name) { }
        }
    }
}
