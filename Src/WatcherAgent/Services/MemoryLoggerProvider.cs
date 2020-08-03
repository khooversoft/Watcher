using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace WatcherAgent.Services
{
    public class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        private readonly int _maxSize;

        public MemoryLoggerProvider(int maxSize)
        {
            maxSize.VerifyAssert(x => maxSize > 0, x => $"{x} must be greater then 0");
            _maxSize = maxSize;
        }

        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}
